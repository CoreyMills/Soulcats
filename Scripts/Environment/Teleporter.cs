using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour {
	
	public float loadingTime = 1f;
	public float spinTargetSpeed = 100f;
	public float spinOutTime = 0.66f;
	public float delayIn = 2f;
	public float delayOut = 1f;
	public float shakeAmount = 0.35f;
	public float shakeTime = 0.66f;
	public string teleportingSound;
	public string teleportSound;

	[HideInInspector]
	public int soulGemsTarget = 0;

	[HideInInspector]
	public int soulGems;

	private int totalNumOfPlayers;
	private LevelLoader levelLoader;
	private int numOfPlayers;
	private bool nextLevel = false;
	private List<Player> teleportedPlayers = new List<Player> ();
	private bool teleporterUsed = false;

	public void Start(){

		// calclate numebr of soul gems required
		Enemy[] enemies = GameObject.Find("Enemies").GetComponentsInChildren<Enemy>();
		foreach (Enemy enemy in enemies) {
			foreach (string itemDrop in enemy.itemDrops) {
				if (itemDrop.Trim () == "SoulGem") {
					soulGemsTarget++;
				}
			}
		}
		
		// total number of players before activating
		totalNumOfPlayers = Game.numberOfPlayers;

		// get level loader
		if (GameObject.Find ("LevelLoader") != null) {
			levelLoader = GameObject.Find ("LevelLoader").GetComponent<LevelLoader> ();
		}
	}

	public IEnumerator OnTriggerEnter(Collider col){
		
		// teleport this player
		if (col.gameObject.tag == "Player" && !teleporterUsed &&
			(soulGemsTarget == 0 || col.attachedRigidbody.gameObject.GetComponent<PlayerInventory>().soulGems > 0)) {
			teleporterUsed = true;
			Player player = col.attachedRigidbody.gameObject.GetComponent<Player> ();

			// disable player input
			player.GetComponent<Player> ().inputEnabled = false;

			// add soul gems one by one
			yield return new WaitForSeconds (0.33f);
			float gemGapTime = col.attachedRigidbody.gameObject.GetComponent<PlayerInventory> ().soulGems < 10f ? 0.25f : 0.1f;
			while (col.attachedRigidbody.gameObject.GetComponent<PlayerInventory> ().soulGems > 0) {

				Audio.Play ("Jey_Audio/SFX/sfx_key_pickup_01", "sfx", 0.5f * Audio.Attenuate(transform.position), 22000f, 1f, 1f);
						
				// add soul gems to teleporter
				soulGems++;

				// remove soul gems from player
				col.attachedRigidbody.gameObject.GetComponent<PlayerInventory> ().soulGems--;


				yield return new WaitForSeconds (gemGapTime);
			}

			// disable player input
			player.GetComponent<Player> ().inputEnabled = true;

			// check if we have enough soul gems to power Teleporter
			if (soulGems >= soulGemsTarget && 
				!teleportedPlayers.Contains(col.attachedRigidbody.gameObject.GetComponent<Player>())) {
				teleportedPlayers.Add (col.attachedRigidbody.gameObject.GetComponent<Player> ());
				StartCoroutine (TeleportPlayer (col.attachedRigidbody.gameObject));
			}
			teleporterUsed = false;
		}
	}

	private IEnumerator TeleportPlayer(GameObject player){

		// disable player input
		player.GetComponent<Player> ().inputEnabled = false;

		// spin camera
		player.GetComponent<Player>().attachedCamera.enabled = false;
		player.GetComponent<Player> ().attachedCamera.transform.gameObject.GetComponentInChildren<Spinner> ().speedTarget = spinTargetSpeed;

		// disable hud
		player.GetComponent<Player>().attachedCamera.hud.Hide();
		player.GetComponent<Player>().attachedCamera.counter.Hide();

		Audio.Play (teleportingSound, "sfx", 1f * Audio.Attenuate(transform.position));

		GameObject particles = (GameObject)GameObject.Instantiate (Resources.Load("Prefabs/Particles/Teleport"));
		particles.transform.position = transform.position + Vector3.up * 0.5f;

		yield return new WaitForSeconds (delayIn);

		Audio.Play (teleportSound, "sfx", 1f * Audio.Attenuate(transform.position));

		GameObject burst = (GameObject)GameObject.Instantiate (Resources.Load("Prefabs/Particles/TeleportBurst"));
		burst.transform.position = transform.position + Vector3.up * 0.5f;

		// shake
		player.GetComponent<Player>().attachedCamera.Shake(shakeAmount,shakeTime, player.transform.position);

		// rumble
		player.GetComponent<Player>().attachedCamera.DeathRumble();

		// remove player
		player.SetActive(false);

		// increment number of players teleported
		numOfPlayers++;
		Debug.Log (numOfPlayers + " out of " + totalNumOfPlayers + " teleported!");

		// check if all players have teleported, then change level
		if (numOfPlayers >= totalNumOfPlayers) {
			Invoke ("Teleport", loadingTime);
		}

		yield return new WaitForSeconds (delayOut);

		player.GetComponent<Player> ().attachedCamera.transform.gameObject.GetComponentInChildren<Spinner> ().speedTargetTime = spinOutTime;
		player.GetComponent<Player> ().attachedCamera.transform.gameObject.GetComponentInChildren<Spinner> ().speedTarget = 0f;
	}

	private void Teleport (){
		
		// telport players to new level
		if (levelLoader != null && !nextLevel) {
			nextLevel = true;
			levelLoader.LoadNextLevel ();
		}
	}		
}
