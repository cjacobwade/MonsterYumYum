using UnityEngine;
using System.Collections;

public class StartScreenUI : MonoBehaviour
{
	public void StartGame()
	{
		Application.LoadLevel( "Test" );
	}
}
