using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
	public int maxHealth = 5;
	public int health = 0;
	public bool respawn = true;
	public float respawnHeight = 15f;
	public string deathSound="";
	public float fallDeathHeight = -20f;

	void Start()
	{
		health = maxHealth;
	}

	void Update () 
	{
		// fall death failsale
		if (transform.position.y < fallDeathHeight) {
			health = 0;
		}

		// if players health is 0
		if(health <= 0)
		{
			// change player position back to its spawn point
			if (respawn) {

				// rumble
				GetComponent<Player>().attachedCamera.DeathRumble();

				// disable input
				gameObject.GetComponent<Player>().inputEnabled = false;

				// hide player
				gameObject.SetActive (false);

				Invoke ("Respawn",1f);
			}

			// kill
			else {
				Invoke ("Die", 0.1f);
			}
		}
	}

	void Respawn(){

		// enable input
		gameObject.GetComponent<Player>().inputEnabled = true;

		// show player
		gameObject.SetActive (true);

		// respawn
		gameObject.transform.position = GameObject.Find ("PlayerSpawnPoint" + gameObject.GetComponent<Player> ().playerId).transform.position +
			Vector3.up * respawnHeight;
		gameObject.GetComponent<Player>().model.transform.rotation = GameObject.Find ("PlayerSpawnPoint" + gameObject.GetComponent<Player> ().playerId).transform.rotation;

		// reset health
		health = maxHealth;

		// reset player
		GetComponent<Player> ().Init ();
	}

	void Die(){
		GameObject.Destroy (gameObject);
	}
}
