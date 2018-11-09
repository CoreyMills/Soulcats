using UnityEngine;
using System.Collections;

public class Cooldown {
	
	public bool cooldown = false;

	private MonoBehaviour host;

	public Cooldown(MonoBehaviour host){
		this.host = host;
	}

	public void StartCooldown(float delay){

		// start cooldown
		cooldown = true;

		// end cooldown after delay
		host.StartCoroutine (EndCooldown (delay));
	}

	public IEnumerator EndCooldown(float delay){

		yield return new WaitForSeconds (delay);

		// end cooldown
		cooldown = false;
	}
}
