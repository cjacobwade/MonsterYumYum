using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsUIManager : MonoBehaviour
{

	[SerializeField] Text _deadCountText = null;
	[SerializeField] Text _timeAliveText = null;

	float _roundTimer = 0f;
	public bool _runTimer = true;

	void Awake()
	{
		FindObjectOfType<VertexPull>().GuyKilled += OnDeadCountChange;
	}

	void Update()
	{
		if ( _runTimer )
		{
			_roundTimer += Time.deltaTime;

			int minutes = (int) _roundTimer / 60;
			int seconds = (int) _roundTimer - ( minutes * 60 );
			string secondsString = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();
			_timeAliveText.text = minutes.ToString() + " : " + secondsString[ 0 ] + " " + secondsString[ 1 ];
		}
	}

	void OnDeadCountChange( int deadCount )
	{
		_deadCountText.text = deadCount.ToString();
	}
}
