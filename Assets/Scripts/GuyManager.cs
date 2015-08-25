using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuyManager : SingletonBehaviour<GuyManager> 
{	
	[SerializeField] GameObject _guyPrefab = null;
	[SerializeField] GameObject _gameOverUI = null;
	[SerializeField] GameObject _gameOverEffect = null;

	int _deadGuys = 0;
	public static int deadGuys
	{
		get { return instance._deadGuys; }
		set { instance._deadGuys = value; }
	}

	bool _gameStopped = false;

	List<Guy> _guyList = new List<Guy>();
	public static List<Guy> guyList
	{
		get 
		{ 
			for( int i = 0; i < instance._guyList.Count; i++ )
			{
				if ( instance._guyList[ i ] == null || !instance._guyList[ i ].gameObject.activeSelf )
				{
					instance._guyList.RemoveAt( i );
				}
			}

			return instance._guyList; 
		}
	}

	public System.Action _GuysClearedCallback = delegate {};
	public static System.Action GuysClearedCallback
	{
		get { return instance._GuysClearedCallback; }
		set { instance._GuysClearedCallback = value; }
	}

	void Awake()
	{
		_guyPrefab.CreatePool( 50 );
	}

	public static Guy SpawnGuy()
	{
		return instance._guyPrefab.Spawn().GetComponent<Guy>();
	}

	public static void DestroyGuy( Guy guy )
	{
		GuyManager.DeregisterGuy( guy );
		guy.Recycle();
	}

	public static void RegisterGuy( Guy guy )
	{
		if ( !guyList.Contains( guy ) )
		{
			guyList.Add( guy );
			guy.FinishedLineCallback += StopGuys;
		}
	}

	public static void DeregisterGuy( Guy guy )
	{
		guyList.Remove( guy );

		if ( guyList.Count == 0 && !instance._gameStopped )
		{
			GuysClearedCallback();
		}
	}

	public static void StopGuys()
	{
		if ( !instance._gameStopped )
		{
			instance._gameStopped = true;
			FindObjectOfType<WaveManager>().StopWave();

			foreach( Guy guy in guyList )
			{
				guy.enabled = false;
			}

			EndGame();

			FindObjectOfType<StatsUIManager>()._runTimer = false;
		}
	}

	public static void EndGame()
	{
		instance._gameOverEffect.SetActive( true );
		instance._gameOverUI.SetActive( true );
	}
}
