using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VertexPull : WadeBehaviour 
{
	#region Mesh Pulling
	MeshFilter _meshFilter = null;

	bool _isPulling = false;
	int _selectedIndex = 0;

	Vector2 _initPullingWorldPos = Vector2.zero;
	Vector2 _currentPullingWorldPos = Vector2.zero;

	[SerializeField] float _maxVertDragDistance = 2f;
	[SerializeField] float _vertSoftSelectRange = 2f;

	[SerializeField] float _snapTime = 0.2f;
	[SerializeField] AnimationCurve _snapCurve = null;

	Vector3[] _meshInitVertPositions = null;

	public System.Action StartPullingCallback = delegate {};
	public System.Action EndPullingCallback = delegate {};

	Coroutine _bounceBackRoutine = null;
	#endregion

	#region Hand
	Transform _handTransform = null;
	SpriteRenderer _handSpriteRenderer = null;

	[SerializeField] Sprite _openHandSprite = null;
	[SerializeField] Sprite _closedHandSprite = null;

	[SerializeField] AudioSource[] _defaultGrabSounds = null;
	#endregion

	#region Little Guys
	List<Guy> _guysToFling = new List<Guy>();
	List<Vector3> _guyInitStopPosition = new List<Vector3>();

	[SerializeField] float _minDistanceToEnableFling = 0.3f;
	[SerializeField] float _guyFlingSpeed = 30f;
	[SerializeField] float _screamChance = 0.3f;

	[SerializeField] AudioSource[] _guyGrabSounds = null;
	[SerializeField] AudioSource[] _guyDeathSounds = null;

	public System.Action<int> GuyKilled = delegate {};
	#endregion

	void Awake()
	{
		// Change animation once we're above the feet
		FindObjectOfType<CameraRise>().AboveFeetCallback += () =>
		{
			animator.Play( "SideToSide" );
		};

		_handTransform = FindObjectOfType<MonsterArmTag>().GetComponent<Transform>();

		_meshFilter = GetComponent<MeshFilter>();
		_handSpriteRenderer = _handTransform.GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		// Hand follow
		Vector2 handFollowPosition = _currentPullingWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		Vector2 toFollowVec = _currentPullingWorldPos - _initPullingWorldPos;
		if ( _isPulling && toFollowVec.magnitude > _maxVertDragDistance )
		{
			handFollowPosition = _initPullingWorldPos + toFollowVec.normalized * _maxVertDragDistance;
		}
		
		_handTransform.position = handFollowPosition.SetZ( _handTransform.position.z );
	}

	void OnMouseDown()
	{
		if ( !_isPulling && _bounceBackRoutine == null )
		{
			_isPulling = true;
			_initPullingWorldPos = _currentPullingWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			_selectedIndex = GetNearestVertIndexToPos( _initPullingWorldPos );

			_meshInitVertPositions = _meshFilter.mesh.vertices;
		
			_handSpriteRenderer.sprite = _closedHandSprite;

			bool foundGuy = false;

			_guysToFling.Clear();
			_guyInitStopPosition.Clear();
			for( int i = 0; i < GuyManager.guyList.Count; i++ )
			{
				Guy guy = GuyManager.guyList[ i ];
				if ( Vector2.Distance( guy.transform.position, _initPullingWorldPos ) < _vertSoftSelectRange )
				{
					foundGuy = true;
					guy.SetMoving( false );
					_guysToFling.Add( guy );
					_guyInitStopPosition.Add( guy.transform.position );
				}
			}

			if ( foundGuy )
			{
				SoundManager.PlaySound( _guyGrabSounds[ Random.Range( 0, _guyGrabSounds.Length ) ] );
			}
			else
			{
				SoundManager.PlaySound( _defaultGrabSounds[ Random.Range( 0, _defaultGrabSounds.Length ) ] );
			}

			StartPullingCallback();
		}
	}

	void OnMouseDrag()
	{
		if ( _isPulling )
		{
			Vector3[] meshVerts = new Vector3[ _meshFilter.mesh.vertexCount ];

			// Drag verts ( and closeby verts towards mouse )
			Vector2 dragVector = Vector2.ClampMagnitude( _currentPullingWorldPos - _initPullingWorldPos, _maxVertDragDistance );
			for( int i = 0; i < _meshFilter.mesh.vertexCount; i++ )
			{
				float distanceFromInitPos = Vector2.Distance( _meshInitVertPositions[ i ], _meshInitVertPositions[ _selectedIndex ] );
				float distanceAlpha = 1f - Mathf.Clamp01( distanceFromInitPos/_vertSoftSelectRange ); // 0-1 value based on distance from selected vert
				meshVerts[ i ] = _meshInitVertPositions[ i ] + (Vector3)dragVector * distanceAlpha;
			}

			// Add any guys that have been created since we started dragging
			for( int i = 0; i < GuyManager.guyList.Count; i++ )
			{
				Guy guy = GuyManager.guyList[ i ];
				if ( !_guysToFling.Contains( guy ) )
				{
					if ( Vector2.Distance( guy.transform.position, _initPullingWorldPos ) < _vertSoftSelectRange )
					{
						guy.SetMoving( false );
						_guysToFling.Add( guy );
						_guyInitStopPosition.Add( guy.transform.position );
					}
				}
			}

			for( int i = 0; i < _guysToFling.Count; i++ )
			{
				Guy guy = _guysToFling[ i ];
				float distanceFromInitPos = Vector2.Distance( _guyInitStopPosition[ i ], _initPullingWorldPos );
				float distanceAlpha = 1f - Mathf.Clamp01( distanceFromInitPos/_vertSoftSelectRange ); // 0-1 value based on distance from selected vert
				guy.transform.position = _guyInitStopPosition[ i ] + (Vector3)dragVector * distanceAlpha;
			}

			_meshFilter.mesh.vertices = meshVerts;
		}
	}

	IEnumerator OnMouseUp()
	{
		if ( _isPulling )
		{
			_isPulling = false;

			_handSpriteRenderer.sprite = _openHandSprite;
			EndPullingCallback();

			int guysFlung = 0;

			// Toss all our guys off
			for( int i = 0; i < _guysToFling.Count; i++ )
			{
				Guy guy = _guysToFling[ i ];
				Vector2 flingVec = _guyInitStopPosition[ i ] - guy.transform.position;

				if ( flingVec.magnitude > _minDistanceToEnableFling )
				{
					guy.rigidbody2D.isKinematic = false;
					guy.rigidbody2D.AddForce( flingVec * flingVec.magnitude/_vertSoftSelectRange * _guyFlingSpeed );
					guy.collider2D.enabled = false;
					GuyManager.DeregisterGuy( guy );

					GuyManager.deadGuys++;
					GuyKilled( GuyManager.deadGuys );

					guysFlung++;
				}
				else
				{
					guy.SetMoving( true );
				}
			}

			List<AudioSource> sourcesToPlay = _guyDeathSounds.ToList();
			for( int i = 0; i < Mathf.Min( guysFlung, 2, _guyDeathSounds.Length ); i++ )
			{
				if ( Random.value > 1f - _screamChance )
				{
					int soundIndex = Random.Range( 0, sourcesToPlay.Count );
					SoundManager.PlaySound( sourcesToPlay[ soundIndex ] );
					sourcesToPlay.RemoveAt( soundIndex );
				}
			}

			if ( _initPullingWorldPos != _currentPullingWorldPos )
			{
				_bounceBackRoutine = StartCoroutine( BounceBackRoutine() );
				yield return _bounceBackRoutine;
			}
		}
	}

	IEnumerator BounceBackRoutine()
	{
		Vector3[] meshBeginSnapVertPositions = _meshFilter.mesh.vertices;
		Vector3[] meshVerts = new Vector3[ _meshFilter.mesh.vertexCount ];

		// Snap all verts back towards their initial position over time
		float snapTimer = 0f;
		while( snapTimer < _snapTime )
		{
			for( int i = 0; i < _meshFilter.mesh.vertexCount; i++ )
			{
				float alpha = _snapCurve.Evaluate( snapTimer/_snapTime );
				meshVerts[ i ] = meshBeginSnapVertPositions[ i ] + ( _meshInitVertPositions[ i ] - meshBeginSnapVertPositions[ i ] ) * alpha; // Unclamped lerp
			}
			
			_meshFilter.mesh.vertices = meshVerts;
			snapTimer += Time.deltaTime;
			yield return null;
		}
		
		// Double check all are snapped back real good like
		for( int i = 0; i < _meshFilter.mesh.vertexCount; i++ )
		{
			meshVerts[ i ] = _meshInitVertPositions[ i ];
		}
		
		_meshFilter.mesh.vertices = meshVerts;
		_bounceBackRoutine = null;
	}

	int GetNearestVertIndexToPos( Vector3 worldPos )
	{
		int nearestVertIndex = 0;
		float nearestVertDist = Vector2.Distance( worldPos, _meshFilter.mesh.vertices[ 0 ] );
		for( int i = 1; i < _meshFilter.mesh.vertexCount; i++ )
		{
			float vertDist = Vector2.Distance( worldPos, _meshFilter.mesh.vertices[ i ] );
			if ( vertDist < nearestVertDist )
			{
				nearestVertIndex = i;
				nearestVertDist = vertDist;
			}
		}

		return nearestVertIndex;
	}

	public Mesh GetMesh()
	{
		return _meshFilter.mesh;
	}

	void OnDrawGizmos()
	{
		if ( _isPulling )
		{
			Gizmos.DrawWireSphere( _currentPullingWorldPos, 0.5f );
			Gizmos.DrawWireSphere( _meshFilter.mesh.vertices[ _selectedIndex ], 0.5f );
		}
	}
}
