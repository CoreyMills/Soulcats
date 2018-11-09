using UnityEngine;
using System.Collections;

public class ActivateMovingBlocksOnTrigger : CameraButton {

	public MovingBlock[] movingBlocks;

	private bool pressed;

	public IEnumerator OnTriggerEnter(Collider c){

		// check it is the player and button not already pressed
		if (c.gameObject.tag == "Player") {

			if (!pressed) {

				// disable input
				if (temporaryCameraTarget != null) {
					c.attachedRigidbody.gameObject.GetComponent<Player> ().inputEnabled = false;
				}

				Audio.Play ("Robin_Audio/Player/sfx_player_ladderstep", "sfx", 1f * Audio.Attenuate(transform.position));
				yield return new WaitForSeconds (0.1f);
				Audio.Play ("Robin_Audio/Player/sfx_player_ladderstep", "sfx", 1f * Audio.Attenuate(transform.position));
				pressed = true;

				// start temporary camera target
				StartCoroutine (StartTemporaryCamera (c.attachedRigidbody.gameObject.GetComponent<Player> ().attachedCamera));

				yield return new WaitForSeconds (
					delayIn * animationDelayMultiplier +
					hudFadeDelay * animationDelayMultiplier);

				// enable moving blocks
				for (int i = 0; i < movingBlocks.Length; i++) {
					movingBlocks [i].enabled = true;
				}
			}
		}
	}
}
