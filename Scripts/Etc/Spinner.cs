using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {

	public float speed = 0f;
	public float speedTarget = 0f;
	public float speedTargetTime = 1f;
	public bool oscillate = false;
	public float oscillateTime = 0f;

	private float spinVelocity = 0f;

	public IEnumerator Start(){

		// oscillate direction
		if (oscillate) {
			while (true) {
				yield return new WaitForSeconds (oscillateTime);
				speedTarget *= -1f;
			}
		}
	}

	public void Update(){

		// smooth spin
		if (speedTarget != -1f) {
			speed = Mathf.SmoothDamp (
				speed,
				speedTarget,
				ref spinVelocity,
				speedTargetTime,
				float.MaxValue,
				Time.deltaTime);
		}

		// spin
		transform.Rotate (Vector3.up, speed * Time.deltaTime,Space.World);
	}
}
