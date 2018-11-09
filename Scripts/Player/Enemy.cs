using UnityEngine;
using System.Collections;

public enum EnemyAIType{
	STATIONARY,
	PASSIVE,
	DEFENSIVE,
	OFFENSIVE
}

public enum EnemyState{
	STILL,
	WANDER,
	FLEE,
	PURSUE
}

public class Enemy : Player 
{
	[Header("ENEMY")]
	public bool sleep = false;
	public bool aiEnabled = true;
	public float aiDisabledMoveTime = 0.1f;
	public string[] itemDrops;
	public float itemDropsThrust = 10f;
	public float itemDropsTorque = 10f;
	public float itemDropsRespawnHeight = 6f;
	public float itemDropRadius = 0f;
	public float deathShakeAmount = 0.3f;
	public float deathShakeTime = 0.5f;
	public float moveDistance = 1f;
	public float moveThrust = 200f;
	public float moveTime = 2f;
	public float checkRayHeight = 0.5f;
	public float checkRayDistance = 1.5f;
	public float checkSphereRadius = 0.45f;
	public float startDelay = 0.5f;
	public LayerMask lineOfSightLayers;
	public bool allowMoveOntoEnemy = false;
	public string disallowMoveOntoEnemyName = "";

	public EnemyAIType ai = EnemyAIType.PASSIVE;
	public EnemyState state = EnemyState.WANDER;

	[Header("DEFENSIVE")]
	public float maxFleeDistance = 4f;
	public float fleeTime = 0.33f;
	public EnemyState defensiveRestingState = EnemyState.STILL;

	[Header("OFFENSIVE")]
	public float maxPursueDistance = 6f;
	public float maxPursueHeightDiff = 2f;
	public float pursueTime = 0.66f;
	public EnemyState offensiveRestingState = EnemyState.STILL;
	public bool useLineOfSight = true;

	[Header("DEATH")]
	public string deathSound = "Jey_Audio/Combat/cmbt_hmn_death_04";
	public string deathParticles = "Particles/Gore";

	public override void Start()
	{
		base.Start();

		// start move enemy loop
		Invoke("MoveEnemy",startDelay);
	}
		
	public override void Update () 
	{
		base.Update ();

		// enemy dead
		if(GetComponent<PlayerHealth>().health <= 0)
		{
			Audio.Play (deathSound, "sfx", 1f * Audio.Attenuate(transform.position));

			GameObject particles = (GameObject)GameObject.Instantiate (Resources.Load ("Prefabs/"+ deathParticles));
			particles.transform.position = transform.position + Vector3.up * 0.5f;

			// drop items
			for(int i = 0; i< itemDrops.Length; i++)
			{
				// spawn position
				Vector3 spawnPosition = gameObject.transform.position;

				// spawn at moving block
				if (lastGroundedMovingBlock != null) {
					spawnPosition = lastGroundedMovingBlock.transform.position + Vector3.up * itemDropsRespawnHeight;
				}

				// spawn drop
				Vector3 offset = new Vector3 (
					                 Random.Range (-itemDropRadius, itemDropRadius),
					                 Random.Range (-itemDropRadius, itemDropRadius),
					                 Random.Range (-itemDropRadius, itemDropRadius));
				GameObject itemDrop = (GameObject) GameObject.Instantiate(Resources.Load("Prefabs/" + itemDrops[i]),
				                                                                spawnPosition + offset,
				                                                                Quaternion.identity);
				// apply force to drop
				Rigidbody rbody = itemDrop.GetComponent<Rigidbody> ();
				if (lastGroundedMovingBlock == null) {
					rbody.AddForce (Vector3.up * itemDropsThrust, ForceMode.Impulse);
				}

				// attach to moving block
				else {
					lastGroundedMovingBlock.GetComponent<MovingBlock> ().pickUps.Add(itemDrop.GetComponent<PickUp> ());
					rbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
				}

				// apply random torque to drop
				rbody.AddTorque (new Vector3 (
					Random.Range (-itemDropsTorque, itemDropsTorque),
					Random.Range (-itemDropsTorque, itemDropsTorque),
					Random.Range (-itemDropsTorque, itemDropsTorque)));

				// set pickup owner last position for respawning
				itemDrop.GetComponent<PickUp>().ownerLastPosition = lastGroundedPosition;
				itemDrop.GetComponent<PickUp> ().ownerLastMovingBlock = lastGroundedMovingBlock;
				itemDrop.GetComponent<PickUp> ().respawnHeight = itemDropsRespawnHeight;
			}

			// shake all cameras
			PlayerCamera.ShakeAll(deathShakeAmount, deathShakeTime, transform.position);

			// destroy enemy
			Destroy(gameObject);
		}
	}

	private void UpdateState(){

		// stationary
		if (ai == EnemyAIType.STATIONARY) {
			state = EnemyState.STILL;
		}

		// passive
		else if (ai == EnemyAIType.PASSIVE) {
			state = EnemyState.WANDER;
		}

		// defensive
		else if (ai == EnemyAIType.DEFENSIVE) {

			// get closest player
			Player closest = GetClosestPlayer ();

			// too close, flee
			if (closest!=null &&
				Vector3.Distance (transform.position, closest.transform.position) <= maxFleeDistance) {
				state = EnemyState.FLEE;
			}

			// wait
			else {
				state = defensiveRestingState;
			}
		}

		// offensive
		else if (ai == EnemyAIType.OFFENSIVE) {

			// get closest player
			Player closest = GetClosestPlayer (true,useLineOfSight);

			// too close, pursue
			if (closest!=null &&
				Vector3.Distance (transform.position, closest.transform.position) <= maxPursueDistance) {
				state = EnemyState.PURSUE;
			}

			// switch back to resting state
			else {
				state = offensiveRestingState;
			}
		}
	}

	private void MoveEnemy()
	{
		// ai enabled
		if (aiEnabled && !sleep) {

			float updatedMoveTime = moveTime;

			UpdateState ();

			// still state
			if (state == EnemyState.STILL) {

				// random direction
				int randomDirection = Random.Range (0, 4);

				// look forward
				if (randomDirection == 0) {
					rotationForward = Vector3.forward;
				}

				// look backward
				else if (randomDirection == 1) {
					rotationForward = Vector3.back;
				}

				// look left
				else if (randomDirection == 2) {
					rotationForward = Vector3.left;
				}

				// look right
				else if (randomDirection == 3) {
					rotationForward = Vector3.right;
				}
			}

			// wander state
			else if (state == EnemyState.WANDER) {

				// random direction
				int randomDirection = Random.Range (0, 4);

				// move forward
				Vector3 position = transform.position;
				if (randomDirection == 0) {
					position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z + moveDistance);
				}

				// move backward
				else if (randomDirection == 1) {
					position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z - moveDistance);
				}

				// move left
				else if (randomDirection == 2) {
					position = new Vector3 (transform.position.x - moveDistance, transform.position.y + checkRayHeight, transform.position.z);
				}

				// move right
				else if (randomDirection == 3) {
					position = new Vector3 (transform.position.x + moveDistance, transform.position.y + checkRayHeight, transform.position.z);
				}

				// attempt to move
				bool moveSuccessful = Move (position);

				// always sprint
				sprint = true;

				// move successful, perform jump
				jump = moveSuccessful;

				// update move time
				updatedMoveTime = moveSuccessful ? moveTime : 0f;
			}

			// flee state
			else if (state == EnemyState.FLEE) {

				// get closest player
				Player closest = GetClosestPlayer ();

				// try all directions, move to position furthest away from closest player
				Vector3 furthestPosition = transform.position;
				float furthestDist = float.MinValue;
				for (int i = 0; i < 5; i++) {

					// move forward
					Vector3 position = transform.position;
					if (i == 0) {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z + moveDistance);
					}

					// move backward
					else if (i == 1) {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z - moveDistance);
					}

					// move left
					else if (i == 2) {
						position = new Vector3 (transform.position.x - moveDistance, transform.position.y + checkRayHeight, transform.position.z);
					}

					// move right
					else if (i == 3) {
						position = new Vector3 (transform.position.x + moveDistance, transform.position.y + checkRayHeight, transform.position.z);
					}

					// stay
					else {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z);
					}

					// check is a valid move
					if (Check (position) && closest != null) {

						// check furthest from player
						float dist = Vector3.Distance (position, closest.transform.position);
						if (dist > furthestDist) {
							furthestDist = dist;
							furthestPosition = position;
						}
					}
				}

				// move
				bool moveSuccessful = false;
				if (furthestPosition != transform.position) {

					// attempt to move
					moveSuccessful = Move (furthestPosition);
					if (rotationForward == Vector3.zero) {
						rotationForward = SnapToWorldAxes (
							(new Vector3 (transform.position.x, 0f, transform.position.z) -
							new Vector3 (closest.transform.position.x, 0f, closest.transform.position.z)).normalized);
					}
				}

				// always sprint
				sprint = true;

				// move successful, perform jump
				jump = moveSuccessful;

				// update move time
				updatedMoveTime = moveSuccessful ? fleeTime : 0f;
			}

			// pursue state
			else if (state == EnemyState.PURSUE) {

				// get closest player
				Player closest = GetClosestPlayer (true, useLineOfSight);

				// try all directions, move to position nearest to closest player
				Vector3 closestPosition = transform.position;
				float closestDist = float.MaxValue;
				for (int i = 0; i < 5; i++) {

					// move forward
					Vector3 position = transform.position;
					if (i == 0) {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z + moveDistance);
					}

					// move backward
					else if (i == 1) {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z - moveDistance);
					}

					// move left
					else if (i == 2) {
						position = new Vector3 (transform.position.x - moveDistance, transform.position.y + checkRayHeight, transform.position.z);
					}

					// move right
					else if (i == 3) {
						position = new Vector3 (transform.position.x + moveDistance, transform.position.y + checkRayHeight, transform.position.z);
					}

					// stay
					else {
						position = new Vector3 (transform.position.x, transform.position.y + checkRayHeight, transform.position.z);
					}

					// check is a valid move
					if (Check (position, true) && closest != null) {

						// check closest to player
						float dist = Vector3.Distance (position, closest.transform.position);
						if (dist < closestDist) {
							closestDist = dist;
							closestPosition = position;
						}
					}
				}

				// move
				bool moveSuccessful = false;
				if (closestPosition != transform.position) {

					// attempt to move
					moveSuccessful = Move (closestPosition, true);
					rotationForward = SnapToWorldAxes (
						(new Vector3 (closest.transform.position.x, 0f, closest.transform.position.z) -
						new Vector3 (transform.position.x, 0f, transform.position.z)).normalized);
				}

				// always sprint
				sprint = true;

				// move successful, perform jump
				jump = moveSuccessful;

				// update move time
				updatedMoveTime = moveSuccessful ? pursueTime : 0f;
			}

			Invoke ("MoveEnemy", updatedMoveTime);
		}

		// ai disabled
		else {
			Invoke ("MoveEnemy", aiDisabledMoveTime);
		}
	}

	private bool Move(Vector3 rayStartPoint, bool allowPlayer = false){

		// check if space to move here
		if (Check (rayStartPoint, allowPlayer)) {
			horizontalTarget = new Vector3 (rayStartPoint.x, 0f, rayStartPoint.z);
			rotationForward = (horizontalTarget - new Vector3 (transform.position.x, 0f, transform.position.z)).normalized;
			return true;
		}
		return false;
	}

	private bool Check(Vector3 rayStartPoint, bool allowPlayer = false){

		// check if floor space to move here
		RaycastHit hit;
		if (Physics.Raycast (rayStartPoint, Vector3.down, out hit, checkRayDistance)) {

			// check space is not filled
			Collider[] overlaps = Physics.OverlapSphere (rayStartPoint, checkSphereRadius);
			foreach(Collider overlap in overlaps){

				// allow target to be player
				if (!allowPlayer || overlap.tag != "Player") {

					// allow target to be enemy
					if (!allowMoveOntoEnemy || overlap.tag != "Enemy") {

						// allow some objects
						if (overlap.tag != "Teleporter" &&
						    overlap.tag != "Button" &&
						    overlap.tag != "SoulGem") {

							// allow self
							if (overlap.attachedRigidbody != GetComponent<Rigidbody> ()) {
								return false;							
							}
						}
					}
				}

				// disallow enemy name
				if (allowMoveOntoEnemy &&
				   disallowMoveOntoEnemyName.Trim ().Length > 0 &&
				   overlap.tag == "Enemy" &&
				   overlap.attachedRigidbody.gameObject.name.Contains (disallowMoveOntoEnemyName.Trim ()) &&
					overlap.attachedRigidbody!=GetComponent<Rigidbody>()) {
					return false;
				}
			}

			// not on a trap
			if (hit.collider.gameObject.GetComponent<Trap> () == null) {				
				return true;
			}
		}

		return false;
	}

	private Player GetClosestPlayer(bool useHeightDiff = false, bool lineOfSight = false){

		// get closest player
		Player[] players = GameObject.Find("Players").GetComponentsInChildren<Player>();
		Player closest = null;
		float closestDist = float.MaxValue;
		foreach (Player p in players) {
			float dist = Vector3.Distance (p.transform.position, transform.position);
			if (dist < closestDist) {

				// check height diff
				if (!useHeightDiff || Mathf.Abs (transform.position.y - p.transform.position.y) <= maxPursueHeightDiff) {

					// check line of sight
					RaycastHit hit;
					if (!lineOfSight ||
					   (Physics.Linecast (
						   transform.position + Vector3.up * checkRayHeight,
						   p.transform.position + Vector3.up * checkRayHeight,
						   out hit,
							lineOfSightLayers) && hit.collider.tag == "Player")) {
						closestDist = dist;
						closest = p;
					}
				}
			}
		}

		return closest;
	}
}
