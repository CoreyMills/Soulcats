using UnityEngine;
using System.Collections;

public class CameraButton : MonoBehaviour {

	public Transform temporaryCameraTarget;
	public float hudFadeDelay = 0.25f;
	public float delayIn = 0.5f;
	public float delayOut = 0.5f;
	public float temporaryCameraDuration = 2f;
	public float animationDelayMultiplier = 1.5f;

	private PlayerCamera currentTemporaryCamera;

	public IEnumerator StartTemporaryCamera(PlayerCamera cam){

		// target exists
		if (temporaryCameraTarget != null) {

			// temporarily disable hud
			cam.hud.Hide();
			cam.counter.Hide();

			// temporarily disable player input
			cam.player.inputEnabled = false;

			// temporarily disable ai
			Enemy[] enemies = GameObject.Find ("Enemies").GetComponentsInChildren<Enemy> ();
			foreach (Enemy enemy in enemies) {
				enemy.aiEnabled = false;
			}

			yield return new WaitForSeconds (delayIn + hudFadeDelay);

			// set temporary camera target
			cam.temporaryTarget = temporaryCameraTarget;

			// set current temporary camera
			currentTemporaryCamera = cam;

			StartCoroutine (EndTemporaryCamera());
		}
	}

	public IEnumerator EndTemporaryCamera(){

		yield return new WaitForSeconds (temporaryCameraDuration);

		// remove temporary camera target
		currentTemporaryCamera.temporaryTarget = null;

		yield return new WaitForSeconds (delayOut);

		// re-enable player input
		currentTemporaryCamera.player.inputEnabled = true;

		// re-enable ai
		Enemy[] enemies = GameObject.Find("Enemies").GetComponentsInChildren<Enemy>();
		foreach (Enemy enemy in enemies) {
			enemy.aiEnabled = true;
		}

		// re-enable hud
		currentTemporaryCamera.hud.Show();
		currentTemporaryCamera.counter.Show();

		// clear current temporary camera
		currentTemporaryCamera = null;
	}
}
