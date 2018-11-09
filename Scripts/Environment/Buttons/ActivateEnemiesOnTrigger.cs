using UnityEngine;
using System.Collections;

public class ActivateEnemiesOnTrigger : MonoBehaviour {

	public Enemy[] enemies;

	private bool pressed;

	public void Start(){

		// initially sleep enemies
		foreach (Enemy enemy in enemies) {
			enemy.sleep = true;
		}
	}

	public void OnTriggerEnter(Collider c){

		// check it is the player and button not already pressed
		if (c.gameObject.tag == "Player") {

			if (!pressed) {

				pressed = true;

				// wake up enemies
				for (int i = 0; i < enemies.Length; i++) {
					enemies [i].sleep = false;
				}
			}
		}
	}
}
