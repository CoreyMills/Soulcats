using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingBlock : MonoBehaviour {

	public Vector3 positionA;
	public Vector3 positionB;
	public float moveTime;
	public float moveInterval;
	public List<Player> players = new List<Player>();
	public List<PickUp> pickUps = new List<PickUp>();
	public bool respawnSoulGem = true;

	private Rigidbody body;
	private bool direction = false;
	private Vector3 previousPosition;

	public IEnumerator Start(){

		// get rigidbody
		body = GetComponent<Rigidbody>();

		// init previous position
		previousPosition = transform.position;

		// loop forever
		while(true){

			// set target position
			Vector3 targetPosition = !direction ? positionB : positionA;

			// animate to target position
			iTween.ValueTo (
				gameObject,
				iTween.Hash (
					"from", transform.position,
					"to", targetPosition,
					"time", moveTime,
					"easeType", "easeInOutSine",
					"onUpdate", "MoveUpdate"));

			// wait until next
			yield return new WaitForSeconds(moveTime);

			// flip direction
			direction = !direction;

			// wait interval
			yield return new WaitForSeconds(moveInterval);
		}
	}

	private void MoveUpdate(Vector3 position){

		// update position
		body.MovePosition(position);
	}

	private void LateUpdate(){

		// propagate movement to player
		foreach (Player player in players) {
			if (player != null) {
				player.transform.position += transform.position - previousPosition;
			}
		}

		// propagate movement to pickup
		foreach (PickUp pickUp in pickUps) {
			if (pickUp != null) {
				pickUp.transform.position += transform.position - previousPosition;
			}
		}

		// update previous position
		previousPosition = transform.position;
	}

	public void OnCollisionEnter(Collision c){

		// collided with soulgem
		if (respawnSoulGem &&
			c.collider.tag == "SoulGem" && 
			c.collider.attachedRigidbody.GetComponent<PickUp>().ownerLastMovingBlock == null &&
			!pickUps.Contains(c.collider.attachedRigidbody.GetComponent<PickUp>())) {
			c.collider.attachedRigidbody.GetComponent<PickUp> ().ownerLastMovingBlock = this;
			c.collider.attachedRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
			pickUps.Add(c.collider.attachedRigidbody.GetComponent<PickUp> ());
		}
	}
}
