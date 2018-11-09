using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	public int strengthMin = 1;
	public int strengthMax = 2;
	public float projectileThrust = 8f;
	public float projectileHitForce =25f;
	public float projectileHitForceUp = 10f;
	public float projectileHitTorqueMax = 10f;
	public float projectileHitShakeAmount = 1f;
	public float projectileHitShakeTime = 0.4f;
	public float disableHitPlayerTime = 0.5f;
	public float nonPlayerModifier = 0.3f;
	public float collisionNormalMax = 30f;
	public float collisionDestroyDelay = 3f;

	private Rigidbody rb;
	private bool dying = false;

	void Start()
	{
		// apply force to projectile
		rb = GetComponent<Rigidbody>();
		rb.AddForce(transform.forward * projectileThrust, ForceMode.Impulse);	 
	}

	void OnCollisionEnter(Collision collision)
	{
		BulletCollide (collision);
	}

	void OnCollisionStay(Collision collision)
	{
		BulletCollide (collision);
	}

	private void BulletCollide(Collision collision)
	{
		// not already dying
		if (!dying) {

			// collision between projectile and player/enemy
			if (collision.collider.attachedRigidbody != null) {
				bool isPlayer = collision.collider.tag == "Player" || collision.collider.tag == "Enemy";

				// apply damage
				if (isPlayer) {
					int strength = Random.Range (strengthMin, strengthMax + 1);
					collision.gameObject.GetComponent<PlayerHealth> ().health = collision.gameObject.GetComponent<PlayerHealth> ().health - strength;
				}

				// temporarily disable hit player movement
				if (collision.collider.tag == "Player") {
					collision.gameObject.GetComponent<Player> ().moveCooldown.StartCooldown (disableHitPlayerTime);
				}

				// calculate modifier for non-player rigidbodies
				float modifier = isPlayer ? 1f : nonPlayerModifier;

				// push back the rigidbody that was hit
				if (!isPlayer || collision.collider.attachedRigidbody.gameObject.GetComponent<Player> ().allowKnockback) {
					collision.collider.attachedRigidbody.AddForce (
						gameObject.transform.forward * projectileHitForce * modifier +
						Vector3.up * projectileHitForceUp * modifier, ForceMode.Impulse); 

					// apply random knockback torque
					collision.collider.attachedRigidbody.AddTorque (new Vector3 (
						Random.Range (-projectileHitTorqueMax, projectileHitTorqueMax),
						Random.Range (-projectileHitTorqueMax, projectileHitTorqueMax),
						Random.Range (-projectileHitTorqueMax, projectileHitTorqueMax)));	
				}

				// shake the camera of the player that was hit
				if (collision.collider.tag == "Player") {
					PlayerCamera cameraShake = collision.gameObject.GetComponent<Player> ().attachedCamera;
					cameraShake.Shake (projectileHitShakeAmount, projectileHitShakeTime, transform.position);
				}
			}

			// test collision normal
			float normal = 
				Mathf.Abs (
					Vector3.Angle (
						transform.forward,
						collision.contacts [0].point - transform.position));
			if (normal < collisionNormalMax || collision.collider.attachedRigidbody != null) {

				dying = true;

				// shake all on collision
				PlayerCamera.ShakeAll (projectileHitShakeAmount * 0.5f, projectileHitShakeTime, transform.position);

				// destroy the projectile
				rb.isKinematic = true;
				//Destroy (gameObject);
				GetComponent<ParticleSystem>().Stop();
				GetComponent<MeshRenderer> ().enabled = false;
				GetComponent<Collider> ().enabled = false;

				GameObject projectileHit = (GameObject)GameObject.Instantiate (Resources.Load("Prefabs/Particles/ProjectileHit"));
				projectileHit.transform.position = transform.position + transform.forward * 0.5f;
			} 
		}
	}
}
