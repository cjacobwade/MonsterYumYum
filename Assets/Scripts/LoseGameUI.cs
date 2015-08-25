using UnityEngine;
using System.Collections;

public class LoseGameUI : MonoBehaviour 
{
	public void RetryLevel()
	{
		Application.LoadLevel( "Test" );
	}

	public void QuitGame()
	{
		Application.LoadLevel ( "StartScreen" );
	}
}
