using UnityEngine;
using System.Collections;

public class PickUp : Body 
{
	public float respawnHeight = 0f;
	public Vector3 ownerLastPosition;
	public MovingBlock ownerLastMovingBlock;

	public virtual void OnCollisionEnter (Collision collision)
	{
		// check collided with player
		if(collision.gameObject.tag == "Player")
		{
			// remove from moving block
			if (ownerLastMovingBlock != null) {
				ownerLastMovingBlock.pickUps.Remove (this);
			}

			Destroy(gameObject);
		}
	}

	public void Respawn(){

		// reset velocity
		body.velocity = Vector3.zero;

		// respawn at owner last position
		if (ownerLastMovingBlock == null) {
			transform.position = ownerLastPosition + Vector3.up * respawnHeight;
			body.constraints = RigidbodyConstraints.None;
		}

		// respawn at owner last moving block
		else {
			transform.position = ownerLastMovingBlock.transform.position + Vector3.up;
			body.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
		}
	}
}