using UnityEngine;
using System.Collections;

public class Body : MonoBehaviour {

	[Header("BODY")]
	public float gravity = -50f;
	public float horizontalDrag = 0.5f;
	public float maxFallSpeed = -20f;

	protected Rigidbody body;

	public virtual void Start(){

		// get rigidbody
		body = GetComponent<Rigidbody>();
	}

	public virtual void FixedUpdate(){

		// apply gravity
		body.AddForce(Vector3.up * gravity, ForceMode.Force);

		// apply horizontal drag
		Vector3 velocity = body.velocity;
		velocity = new Vector3 (
			velocity.x * horizontalDrag,
			velocity.y,
			velocity.z * horizontalDrag);
		body.velocity = velocity;

		// apply max speed
		body.velocity = new Vector3 (
			body.velocity.x,
			Mathf.Max (body.velocity.y, maxFallSpeed),
			body.velocity.z);
	}
}
