using UnityEngine;
using System.Collections;

public class Guy : WadeBehaviour 
{
	BezierSpline _bezierSpline = null;

	[SerializeField] float _normalSpeed = 0.02f;
	[SerializeField] float _catchupSpeed = 0.1f;
	float _appliedMoveSpeed = 0f;

	float _percentAlongSpline = 0f;

	public System.Action FinishedLineCallback = delegate {};

	void Awake()
	{
		Initialize();
	}

	void Update()
	{
		_percentAlongSpline += _appliedMoveSpeed * Time.deltaTime;
		transform.position = _bezierSpline.GetPoint( _percentAlongSpline );

		if ( _percentAlongSpline >= 1f )
		{
			FinishedLineCallback();
			GuyManager.DestroyGuy( this );
		}
	}

	public void Initialize()
	{
		GuyManager.RegisterGuy( this );

		BezierSpline[] splines = FindObjectsOfType<BezierSpline>();
		_bezierSpline = splines[ Random.Range( 0, splines.Length ) ];

		collider2D.enabled = true;
		rigidbody2D.isKinematic = true;
		transform.position = _bezierSpline.GetPoint( 0f );

		_appliedMoveSpeed = _catchupSpeed;
		_percentAlongSpline = 0f;
		enabled = true;
	}

	public void SetNormalSpeed( float setSpeed )
	{
		_normalSpeed = setSpeed;
	}

	public void SlowDown()
	{
		_appliedMoveSpeed = _normalSpeed;
	}

	public void SetMoving( bool setMoving )
	{
		enabled = setMoving;
	}
}
