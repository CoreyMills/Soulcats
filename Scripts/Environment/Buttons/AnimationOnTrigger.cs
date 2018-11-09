using UnityEngine;
using System.Collections;

public enum TriggerType
{
	PRESS,
	HOLD
}

public class AnimationOnTrigger : CameraButton {

	public TriggerType type;
	public Animation[] animationToPlay;
	public AnimationOnTrigger depends;

	public bool pressed;

	public IEnumerator OnTriggerEnter(Collider c){

		// check it is the player and button not already pressed
		if (c.tag == "Player") {

			if (!pressed) {

				// disable input
				if (temporaryCameraTarget != null) {
					c.attachedRigidbody.gameObject.GetComponent<Player> ().inputEnabled = false;
				}

				Audio.Play ("Robin_Audio/Player/sfx_player_ladderstep", "sfx", 1f * Audio.Attenuate(transform.position));
				yield return new WaitForSeconds (0.1f);
				Audio.Play ("Robin_Audio/Player/sfx_player_ladderstep", "sfx", 1f * Audio.Attenuate(transform.position));
				pressed = true;			

				// check if other button is pressed
				if (depends == null || depends.pressed) {

					// start temporary camera target
					StartCoroutine (StartTemporaryCamera (c.attachedRigidbody.gameObject.GetComponent<Player> ().attachedCamera));

					yield return new WaitForSeconds (
						delayIn * animationDelayMultiplier +
						hudFadeDelay * animationDelayMultiplier);

					// play animation
					Play ();
					Audio.Play ("Jey_Audio/SFX/sfx_skull_tilt_01", "sfx", 0.5f * Audio.Attenuate(animationToPlay[0].transform.position));

					// play other animation
					if (depends != null) {
						depends.Play ();
					}
				}
			}
		}
	}

	public void OnTriggerExit(Collider c){

		// check if player unpressed button
		if (c.tag == "Player") {
			if(type == TriggerType.HOLD){
				pressed = false;
			}
		}
	}

	public void Play(){

		// play all animations
		foreach(Animation a in animationToPlay){
			a.Play();
		}
	}
}
