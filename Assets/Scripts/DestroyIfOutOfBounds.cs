using UnityEngine;
using System.Collections;

public class DestroyIfOutOfBounds : MonoBehaviour 
{
	void Update () 
	{
		if ( transform.position.y < -100f )
		{
			GuyManager.DestroyGuy( GetComponent<Guy>() );
		}
	}
}
