using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Rigidbody2D ) )]
public class PositionalSpring : MonoBehaviour
{
	[SerializeField] Vector2 _anchorPoint = Vector2.zero;
	[SerializeField] float _springConstant = 1.0f;
	[SerializeField] bool _simulateLocal = true;

	Transform _transform;
	Rigidbody2D _rigidBody;

	void Awake()
	{
		_transform = GetComponent<Transform>();
		_rigidBody = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		Vector2 offset = _anchorPoint - (Vector2)( _simulateLocal ? _transform.localPosition : _transform.position );
		_rigidBody.AddForce( offset * _springConstant );
	}
}
