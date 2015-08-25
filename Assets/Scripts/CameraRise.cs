using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRise : WadeBehaviour
{
	[SerializeField] float _riseSpeed = 20f;
	List<Guy> _hitGuys = new List<Guy>();

	bool _shouldRise = false;

	[SerializeField] float _shouldRiseTime = 1f;
	float _shouldRiseTimer = 0f;

	[SerializeField] float _aboveFeetHeight = 10f;
	bool _aboveFeet = false;

	[SerializeField] float _atHeadHeight = 50f;

	[SerializeField] float _zoomOutCameraSize = 2f;
	[SerializeField] float _zoomOutTime = 3f;

	public System.Action AboveFeetCallback = delegate {};
	public System.Action AtHeadCallback = delegate {};
	
	void Update()
	{
		if ( _hitGuys.Count > 0 )
		{
			_shouldRise = true;
		}
		else if ( _shouldRise )
		{
			_shouldRiseTimer += Time.deltaTime;
			if ( _shouldRiseTimer >= _shouldRiseTime )
			{
				_shouldRiseTimer = 0f;
				_shouldRise = false;
			}
		}

		if ( _shouldRise )
		{
			transform.position += transform.up * _riseSpeed * Time.deltaTime;
		}

		if ( !_aboveFeet && transform.position.y > _aboveFeetHeight )
		{
			AboveFeetCallback();
			_aboveFeet = true;
		}

		if ( transform.position.y > _atHeadHeight )
		{
			AtHeadCallback();
			StartCoroutine( ZoomOutRoutine() );
			enabled = false;
		}
	}

	void OnTriggerStay2D( Collider2D other )
	{
		Guy guy = other.GetComponent<Guy>();
		if ( guy && !_hitGuys.Contains( guy ) )
		{
			_hitGuys.Add( guy );
		}
	}

	void OnTriggerExit2D( Collider2D other )
	{
		Guy guy = other.GetComponent<Guy>();
		if ( guy )
		{
			_hitGuys.Remove( guy );

			if ( guy.transform.position.y > transform.position.y + collider2D.offset.y )
			{
				GuyManager.DeregisterGuy( guy );
				guy.Recycle();
			}
		}
	}

	IEnumerator ZoomOutRoutine()
	{
		float startZoomSize = camera.orthographicSize;

		float zoomOutTimer = 0f;
		while( zoomOutTimer < _zoomOutTime )
		{
			camera.orthographicSize = Mathf.Lerp( startZoomSize, _zoomOutCameraSize, zoomOutTimer/_zoomOutTime );
			zoomOutTimer += Time.deltaTime;
			yield return null;
		}

		camera.orthographicSize = _zoomOutCameraSize;

		// Increase size of slowdown box
		BoxCollider2D slowBox = GetComponentInChildren<SlowGuysZone>().GetComponent<BoxCollider2D>();

		Vector2 slowBoxSize = slowBox.size;
		slowBoxSize.y += 1.3f;

		slowBox.size = slowBoxSize;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Vector3 centerPos = Vector3.up * _aboveFeetHeight;
		Gizmos.DrawLine( centerPos - Vector3.right * 50f, centerPos + Vector3.right * 50f );

		Gizmos.color = Color.red;
		centerPos = Vector3.up * _atHeadHeight;
		Gizmos.DrawLine( centerPos - Vector3.right * 50f, centerPos + Vector3.right * 50f );
	}
}
