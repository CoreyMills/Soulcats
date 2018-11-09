using UnityEngine;
using System.Collections;

public enum TrapParticleType{
	SURFACE,
	BODY
}

public class Trap : MonoBehaviour 
{
	public int damage = 10;
	public float shakeAmount = 0.3f;
	public float shakeTime = 0.5f;
	public float cooldownTime = 1f;
	public bool affectsEnemies = true;
	public bool respawnSoulGems = true;
	public string ignoreName = "";
	public string trapSound = "";
	public string trapSound2 = "";
	public string particlesPath = "";
	public TrapParticleType particleType = TrapParticleType.SURFACE;

	private Cooldown trapCooldown;

	void Start(){
		trapCooldown = new Cooldown (this);
	}

	void OnTriggerStay(Collider collider)
	{

		// collided with player/enemy
		if ((collider.gameObject.tag == "Player" || collider.gameObject.tag == "Enemy") && 
			!trapCooldown.cooldown) {

			// ignore name
			if (ignoreName.Trim().Length > 0 && 
				collider.attachedRigidbody.gameObject.name.Contains (ignoreName.Trim())) {
				return;
			}

			// start cooldown
			trapCooldown.StartCooldown(cooldownTime);

			// damage player
			if (affectsEnemies || collider.gameObject.tag == "Player") {

				// add particles
				if (particlesPath.Trim ().Length > 0) {
					RaycastHit hit;
					Physics.Raycast (
						collider.transform.position + Vector3.up * 0.5f,
						Vector3.down,
						out hit,
						float.MaxValue);

					GameObject particles = (GameObject)GameObject.Instantiate (Resources.Load ("Prefabs/" + particlesPath));
					if (particleType == TrapParticleType.SURFACE) {
						particles.transform.position = hit.point;
					} else {
						particles.transform.position = collider.bounds.center;
					}
				}

				Audio.Play (trapSound, "sfx", 1f * Audio.Attenuate(collider.transform.position));
				Audio.Play (trapSound2, "sfx", 0.75f * Audio.Attenuate(collider.transform.position));
				collider.attachedRigidbody.gameObject.GetComponent<PlayerHealth> ().health -= damage;
			}

			// screen shake
			if (collider.attachedRigidbody.gameObject.GetComponent<Player> ().attachedCamera != null) {
				collider.attachedRigidbody.gameObject.GetComponent<Player> ().attachedCamera.Shake (shakeAmount, shakeTime, transform.position);
			}
		}

		// collided with soul gem
		else if (collider.gameObject.tag == "SoulGem" &&
			respawnSoulGems){
			collider.gameObject.GetComponent<SoulGem> ().Respawn ();
		}
	}
}
