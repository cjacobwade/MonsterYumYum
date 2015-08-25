using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ColorPair
{
	public Color furColor = Color.white;
	public Color skinColor = Color.white;
}

public class MonsterGenerator : WadeBehaviour 
{
	#region Body Stitching
//	int _horizontalQuads = 9;
//	int _verticalQuads = 7; // Vertical quads per monster piece

	int _horizontalQuads = 19;
	int _verticalQuads = 15; // Vertical quads per monster piece

	[SerializeField] int _bodyLength = 5;
	[SerializeField] Texture2D[] _topTextures = null;
	[SerializeField] Texture2D[] _bodyTextures = null;
	[SerializeField] Texture2D[] _feetTextures = null;

	Texture2D _stitchedTexture = null;
	[SerializeField] float _bodyScale = 0.3f;
	#endregion

	#region Path Creation
	[SerializeField] int _numPathsToCreate = 8;
	[SerializeField] float _spawnXOffset = 3f;
	[SerializeField] float _footSpawnXOffset = 3f;

	[SerializeField] Vector2 _boxSizeScale = new Vector2( 1.3f, 1.05f );
	#endregion

	[SerializeField] ColorPair[] _colorPairs = null;

	void Awake()
	{
		float xCenterOffset = _bodyScale * (float)( _horizontalQuads - 1 )/2f;

		// Generate mesh from piece info
		MeshFilter meshFilter = GetComponent<MeshFilter>();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();

		int appliedVerticalQuads = _verticalQuads * _bodyLength;
		for( int y = 0; y < appliedVerticalQuads; y++ )
		{
			for ( int x = 0; x < _horizontalQuads; x++ )
			{
				vertices.Add( new Vector3( x * _bodyScale - xCenterOffset, y * _bodyScale ) );
				uvs.Add( new Vector2( x/(float)( _horizontalQuads - 1 ), y/(float)( appliedVerticalQuads - 1 ) ) );

				if ( x != 0 && y != 0 )
				{
					int topRight = y * _horizontalQuads + x;
					int topLeft = y * _horizontalQuads + x - 1;
					int bottomRight = ( y - 1 ) * _horizontalQuads + x;
					int bottomLeft = ( y - 1 ) * _horizontalQuads + x - 1;

					triangles.Add( bottomLeft );
					triangles.Add( topLeft );
					triangles.Add( topRight );

					triangles.Add( bottomRight );
					triangles.Add( bottomLeft );
					triangles.Add( topRight );
				}
					
			}
		}

		Mesh mesh = meshFilter.mesh;
		mesh.Clear();

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.UploadMeshData( false );

		// Generate texture from pieces

		List<Texture2D> texturesToStitch = new List<Texture2D>();
		texturesToStitch.Add( _feetTextures[ Random.Range( 0, _feetTextures.Length ) ] );
		for( int i = 0; i < _bodyLength - 2; i++ )
		{
			texturesToStitch.Add( _bodyTextures[ Random.Range( 0, _bodyTextures.Length ) ] );
		}
		texturesToStitch.Add( _topTextures[ Random.Range( 0, _topTextures.Length ) ] );

		int totalHeight = _bodyLength * texturesToStitch[ 0 ].height;

		int prevY = 0;
		_stitchedTexture = new Texture2D( texturesToStitch[ 0 ].width, totalHeight );
		for( int i = 0; i < texturesToStitch.Count; i++ )
		{
			_stitchedTexture.SetPixels( 0, prevY, texturesToStitch[ i ].width, texturesToStitch[ i ].height, texturesToStitch[ i ].GetPixels() );
			prevY += texturesToStitch[ i ].height;
		}

		_stitchedTexture.Apply();
		renderer.material.SetTexture( "_MainTex", _stitchedTexture );

		int colorPairIndex = Random.Range( 0, _colorPairs.Length );
		renderer.material.SetColor( "_FurColor", _colorPairs[ colorPairIndex ].furColor );
		renderer.material.SetColor( "_SkinColor", _colorPairs[ colorPairIndex ].skinColor );

		Renderer handRenderer = FindObjectOfType<MonsterArmTag>().renderer;
		handRenderer.material.SetColor( "_FurColor", _colorPairs[ colorPairIndex ].furColor );
		handRenderer.material.SetColor( "_SkinColor", _colorPairs[ colorPairIndex ].skinColor );

		// Generate paths for guys to travel along
		// Fuck this forever
		for( int i = 0; i < _numPathsToCreate; i++ )
		{
			GameObject guyPathObj = new GameObject( "GuyPath", typeof( BezierSpline ) );
			guyPathObj.transform.parent = transform;
			BezierSpline guyPathSpline = guyPathObj.GetComponent<BezierSpline>();

			guyPathSpline.Reset();
			for( int j = 0; j < texturesToStitch.Count - 1; j++ )
			{
				guyPathSpline.AddCurve();
			}

			for( int j = 0; j < guyPathSpline.ControlPointCount; j++ )
			{
				if ( j == 0 )
				{
					// Set at one of the foot points
					guyPathSpline.SetControlPoint( j, Vector3.zero + Vector3.right * _footSpawnXOffset * Mathf.Sign( Random.value - 0.5f ) );
					guyPathSpline.SetControlPoint( j + 1, guyPathSpline.GetControlPoint( j ) + Vector3.up );
				}
				else if ( j == guyPathSpline.ControlPointCount - 1 )
				{
					// Last point
					guyPathSpline.SetControlPoint( j, Vector3.up * _bodyScale * ( appliedVerticalQuads - _verticalQuads/2f ) );
					guyPathSpline.SetControlPoint( j - 1, guyPathSpline.GetControlPoint( j ) - Vector3.up );
				}
				else if ( j % 3 == 0 )
				{
					guyPathSpline.SetControlPoint( j, Vector3.up * j/3f * _verticalQuads * _bodyScale + Vector3.right * Random.Range( -_spawnXOffset, _spawnXOffset ) );

					guyPathSpline.SetControlPointMode( j, BezierControlPointMode.Mirrored );
					guyPathSpline.SetControlPoint( j + 1, guyPathSpline.GetControlPoint( j ) + Vector3.up );
				}
			}
		}

		// Setup collider
		BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();

		Vector2 boxScale = boxCollider.size;
		boxScale.Scale( _boxSizeScale );

		boxCollider.size = boxScale;
	}
}
