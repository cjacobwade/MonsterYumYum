using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour 
{
	int currentWave = 0;
	[SerializeField] AnimationCurve _enemyCountCurve = null;
	[SerializeField] AnimationCurve _enemySpawnTimeCurve = null;
	[SerializeField] AnimationCurve _enemySpeedCurve = null;

	public System.Action StartWaveCallback = delegate {};
	public System.Action WaveLostCallback = delegate {};

	Coroutine _waveRoutine = null;

	void Awake()
	{
		StartNextWave();
	}

	public void StartNextWave()
	{
		GuyManager.GuysClearedCallback -= StartNextWave;

		if ( _waveRoutine != null )
		{
			StopCoroutine( _waveRoutine );
		}

		_waveRoutine = StartCoroutine( WaveRoutine( currentWave++ ) );
	}

	IEnumerator WaveRoutine( int waveNum )
	{
		StartWaveCallback();

		int numSpawned = 0;
		while( numSpawned < _enemyCountCurve.Evaluate( waveNum ) )
		{
			yield return new WaitForSeconds( _enemySpawnTimeCurve.Evaluate( waveNum ) );
			Guy spawnedGuy = GuyManager.SpawnGuy();
			spawnedGuy.Initialize();
			spawnedGuy.FinishedLineCallback -= WaveLostCallback;
			spawnedGuy.FinishedLineCallback += WaveLostCallback;
			spawnedGuy.SetNormalSpeed( _enemySpeedCurve.Evaluate( waveNum ) );
			numSpawned++;
		}

		GuyManager.GuysClearedCallback += StartNextWave;
		_waveRoutine = null;
	}

	public void StopWave()
	{
		if ( _waveRoutine != null )
		{
			StopCoroutine( _waveRoutine );
		}
	}
}
