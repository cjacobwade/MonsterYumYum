using UnityEngine;
using System.Collections;

public class GuyRandomizer : WadeBehaviour 
{
	[SerializeField] Sprite[] _weaponSprites = null;
	[SerializeField] Color[] _skinColors = null;

	void Awake()
	{
		renderer.material.SetColor( "_SkinColor", _skinColors[ Random.Range( 0, _skinColors.Length ) ] );
		transform.GetChild( 0 ).GetComponent<SpriteRenderer>().sprite = _weaponSprites[ Random.Range( 0, _weaponSprites.Length ) ];
	}
}
