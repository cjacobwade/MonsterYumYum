using UnityEngine;
using System.Collections;

public class SlowGuysZone : WadeBehaviour 
{
	void OnTriggerEnter2D( Collider2D other )
	{
		Guy guy = other.GetComponent<Guy>();
		if ( guy )
		{
			guy.SlowDown();
		}
	}
}
