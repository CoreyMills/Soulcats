using UnityEngine;
using System.Collections;

public enum StickRange{
	FOURWAY,
	EIGHTWAY,
	ANALOG360
}

public enum PlayerClass{
	LAUNCHER,
	HAMMER,
	NONE
}

public class Player : Body {

	[Header("CAMERA")]
	public PlayerCamera attachedCamera;

	[Header("MODEL")]
	public GameObject model;

	[Header("INPUT")]
	public bool inputEnabled = true;
	public int playerId = 0;
	public float axisThreshold = 0.33f;

	[Header("SENSORS")]
	public Vector3[] sensorPositions;
	public float sensorExtends = 0.35f;
	public float sensorThickness = 0.01f;
	public float lastGroundedY = 0f;
	public float lastGroundedCheckHeight = 0.1f;
	public float lastGroundedCheckDistance = 0.11f;

	[Header("MOVEMENT")]
	public bool debug = false;
	public StickRange movementRange;
	public StickRange aimRange;
	public bool snapToGrid = true;
	public float horizontalDistance = 1f;
	public float horizontalForce = 200f;
	public float jumpForce = 12f;
	public float jumpForceSprint = 6f;
	public float jumpCooldownTime = 0.5f;
	public float jumpCooldownTimeSprint = 0.2f;
	public float rotationTime = 0.15f;
	public float minTimeShake = 0.5f;
	public float maxTimeShake = 1f;
	public float minShake = 0f;
	public float maxShake = 1f;
	public float shakeTime = 0.2f;
	public Cooldown moveCooldown;
	public bool allowKnockback = true;

	[Header("JELLY")]
	public bool jelly = true;
	public float jumpTime = 0.05f;
	public float jumpSquash = -0.25f;
	public float jumpStretch = 0.25f;
	public float fallVelocityThreshold = -2f;
	public float fallTime = 0.1f;
	public float fallSquash = 0.33f;
	public float fallStretch = -0.33f;
	public float idleDelay = 0.5f; 
	public float idleTime = 0.75f;
	public float idleSquash = -0.25f;
	public float idleStretch = 0.25f;

	[Header("ATTACKS")]
	public int spawnDamage = 0;
	public float spawnDamageRadius = 1.5f;
	public PlayerClass playerClass;
	public GameObject weapon;

	[Header("LAUNCHER")]
	public string projectileName;
	public float fireRate = 1f;
	public float startPointZ = 0.5f;
	public float startPointY = 0.75f;
	public float shakeAmount = 0.75f;
	public float projectileShakeTime = 0.2f;
	public float gunRecoil = 10f;
	public float gunRecoilUp = 0f;
	public float gunMoveCooldownTime = 0.5f;
	public LayerMask muzzleCheckLayers;

	[Header("HAMMER")]
	public int hammerDamageMin = 3;
	public int hammerDamageMax = 3;
	public float hammerRate = 1.1f;
	public float hammerHitHeight = 0.5f;
	public float hammerHitRadius = 1.25f;
	public float hammerAnimationTime = 0.5f;
	public float hammerShake = 0.5f;
	public float hammerShakeTime = 0.25f;
	public float hammerForce = 25f;
	public float hammerForceUp = 10f;
	public float hammerFreezeTime = 0.1f;
	public float hammerDisableHitPlayerTime = 0.5f;
	public float hammerNonPlayerModifier = 0.3f;
	public float hammerNonPlayerModifierUp = 0.3f;
	public float hammerTorqueMax = 10f;

	[Header("AUDIO")]
	public string jumpSound = "";
	public string landSound = "";
	public string hardLandSound = "";
	public string hammerSwing = "";
	public string hammerDown = "";
	public string hammerDown2 = "";
	public string hammerDown3 = "";
	public string shoot1 = "";

	private bool grounded = false;
	protected bool jump = false;
	protected bool sprint = false;
	private bool idle = false;
	private bool idleState = false;
	private float idleDelayWait = 0f;
	protected Vector3 horizontalTarget;
	private Cooldown jumpCooldown;
	private Vector3 modelScaleTarget;
	private Vector3 modelScaleVelocity;
	protected Vector3 rotationForward;
	private float rotationVelocity;
	private MovingBlock attachedMovingBlock;
	private float fallDuration = 0f;
	protected Vector3 lastGroundedPosition;
	protected MovingBlock lastGroundedMovingBlock;
	private bool firstSpawn = true;
	private bool firstGrounded = true;

	private bool attack = false;
	private bool hammering = false;
	private Cooldown shootCooldown;
	private Cooldown hammerCooldown;

	private PlayerXInput xInput = new PlayerXInput();

	public override void Start()
	{
		base.Start ();
		Init();
	}

	public void Init(){

		// init first spawn
		firstSpawn = true;
		
		// init horizontal target
		horizontalTarget = new Vector3(transform.position.x, 0f, transform.position.z);
		
		// init rotation forward
		rotationForward = model.transform.forward;
		
		// init cooldowns
		jumpCooldown = new Cooldown(this);
		moveCooldown = new Cooldown(this);

		// launcher
		if (playerClass == PlayerClass.LAUNCHER) {
			shootCooldown = new Cooldown (this);
			StartCoroutine (ShootBullet ());
		}

		// hammer
		else if (playerClass == PlayerClass.HAMMER) {
			hammering = false;
			hammerCooldown = new Cooldown (this);
			StartCoroutine (SwingHammer ());	

			// reset hammer animation
			weapon.GetComponentInChildren<Animation>().Rewind();
			weapon.GetComponentInChildren<Animation>().Play();
			weapon.GetComponentInChildren<Animation>().Sample();
			weapon.GetComponentInChildren<Animation>().Stop();
		}
	}

	public IEnumerator SwingHammer()
	{
		// loop forever
		while(true){

			// attack
			if(attack && !hammerCooldown.cooldown && grounded && !jump && !jumpCooldown.cooldown && !firstSpawn)
			{
				// start hammering
				hammering = true;

				// start hammer cooldown
				hammerCooldown.StartCooldown(hammerRate);

				// jump up
				jump = true;

				// disable sprint
				sprint = false;

				// play hammer animation
				weapon.GetComponentInChildren<Animation>().Play();

				StartCoroutine (HammerDown ());

				Audio.Play (hammerSwing, "sfx", 1f * Audio.Attenuate(transform.position));
			}

			yield return null;
		}
	}

	public IEnumerator HammerDown(){
		
		yield return new WaitForSeconds (hammerAnimationTime);

		// done hammering
		hammering = false;

		// rumble
		if (inputEnabled) {
			StartCoroutine(xInput.Rumble (playerId, 1000f, 0.33f));
		}

		// shake all on hammer down
		PlayerCamera.ShakeAll (hammerShake, hammerShakeTime, transform.position);

		Audio.Play (hammerDown, "sfx", 1f * Audio.Attenuate(transform.position));
		Audio.Play (hammerDown2, "sfx", 0.5f * Audio.Attenuate(transform.position));
		Audio.Play (hammerDown3, "sfx", 0.25f * Audio.Attenuate(transform.position));

		// hit objects
		Collider[] hits = Physics.OverlapSphere(transform.position + rotationForward + Vector3.up * hammerHitHeight, hammerHitRadius);

		GameObject hammerDownParticles = (GameObject)GameObject.Instantiate (Resources.Load("Prefabs/Particles/HammerDown"));
		hammerDownParticles.transform.position = transform.position + rotationForward;

		if (hits.Length > 0) {
			foreach (Collider hit in hits) {

				// add force to rigidbody
				if (hit.attachedRigidbody != null && hit != GetComponentInChildren<Collider> () &&
					hit.tag != "SoulGem") {			

					// hit player
					bool isPlayer = hit.tag == "Player" || hit.tag == "Enemy";

					// calculate modifier for non player rigidbodies
					float modifier = isPlayer ? 1f : hammerNonPlayerModifier;
					float modifierUp = isPlayer ? 1f : hammerNonPlayerModifierUp;

					// add knockback force
					if (!isPlayer || hit.attachedRigidbody.gameObject.GetComponent<Player> ().allowKnockback) {
						hit.attachedRigidbody.AddForce (
							(hit.attachedRigidbody.transform.position - transform.position).normalized * hammerForce * modifier +
							Vector3.up * hammerForceUp * modifierUp, ForceMode.Impulse);	

						// apply random knockback torque
						hit.attachedRigidbody.AddTorque (new Vector3 (
							Random.Range (-hammerTorqueMax, hammerTorqueMax),
							Random.Range (-hammerTorqueMax, hammerTorqueMax),
							Random.Range (-hammerTorqueMax, hammerTorqueMax)));
					}

					// apply damage
					if (isPlayer) {
						int hammerDamage = Random.Range (hammerDamageMin, hammerDamageMax + 1);
						hit.attachedRigidbody.gameObject.GetComponent<PlayerHealth> ().health = hit.attachedRigidbody.gameObject.GetComponent<PlayerHealth> ().health - hammerDamage;
						hit.attachedRigidbody.gameObject.GetComponent<Player> ().moveCooldown.StartCooldown (hammerDisableHitPlayerTime);
					}

					// shake the camera of the player that was hit
					if (hit.tag == "Player") {
						PlayerCamera cameraShake = hit.attachedRigidbody.gameObject.GetComponent<Player> ().attachedCamera;
						cameraShake.Shake (hammerShake, hammerShakeTime, hit.attachedRigidbody.transform.position);
					}
				}
			}

			// freeze frame
			//Time.timeScale = 0f;
		}

		// resume timescale
		yield return Wait.ForRealSeconds (hammerFreezeTime);
		//Time.timeScale = 1f;
	}

	public IEnumerator ShootBullet()
	{
		// loop forever
		while(true){

			// attack
			if(attack && !shootCooldown.cooldown && !firstSpawn)
			{
				// check nothing in the way of muzzle
				Vector3 muzzlePosition = transform.position + rotationForward * startPointZ + Vector3.up * startPointY;
				if(!Physics.Linecast(
					transform.position + Vector3.up * startPointY,
					muzzlePosition,
					muzzleCheckLayers)){

					// instantiate projectile
					GameObject projectile = (GameObject) Instantiate(Resources.Load("Prefabs/" + projectileName),
						muzzlePosition,
						Quaternion.LookRotation(rotationForward));

					// make bullet ignore its owner
					Physics.IgnoreCollision(GetComponentInChildren<Collider>(), projectile.GetComponent<Collider>());
				}

				// rumble
				if (inputEnabled) {
					StartCoroutine(xInput.Rumble (playerId, 1000f, 0.25f));
				}

				// gun shake
				PlayerCamera.ShakeAll(shakeAmount, projectileShakeTime, transform.position);

				// apply recoil
				body.AddForce((rotationForward * -1f) * gunRecoil + Vector3.up * gunRecoilUp, ForceMode.Impulse); 
				moveCooldown.StartCooldown (gunMoveCooldownTime);

				// start shooting cooldown
				shootCooldown.StartCooldown(fireRate);

				Audio.Play (shoot1, "sfx", 0.75f * Audio.Attenuate(transform.position));
			}

			yield return null;
		}
	}

	public void HandleInput (float lsv, float lsh, float rsv, float rsh, bool crl, bool crr, bool sprint, bool weapon)
	{
		// allow player inpit
		if (inputEnabled) {
			
			// shoot weapon
			attack = weapon;

			// toggle sprint
			this.sprint = sprint;

			// disable sprinting during hammering
			if (hammering) {
				this.sprint = false;
			}

			// allow jump if grounded and not already jumping
			if (grounded && !jump && !jumpCooldown.cooldown) {

				Vector3 targetDirection = Vector3.zero;

				// axis off centre
				if (Mathf.Abs (lsv) >= axisThreshold || Mathf.Abs (lsh) >= axisThreshold) {

					// activate jump
					jump = true;

					// forward/backward
					if (Mathf.Abs (lsv) >= axisThreshold) {
						targetDirection = 
							(movementRange == StickRange.FOURWAY ? Vector3.zero : targetDirection) +
						SnapToWorldAxes (attachedCamera.cam.transform.forward) *
						(movementRange == StickRange.ANALOG360 ? lsv : Mathf.Sign (lsv));
					}

					// left/right
					if (Mathf.Abs (lsh) >= axisThreshold) {
						targetDirection = 
							(movementRange == StickRange.FOURWAY ? Vector3.zero : targetDirection) +
						SnapToWorldAxes (attachedCamera.cam.transform.right) *
						(movementRange == StickRange.ANALOG360 ? lsh : Mathf.Sign (lsh));
					}
				}

				// valid target direction
				if (targetDirection != Vector3.zero) {

					// update rotation forward
					rotationForward = targetDirection.normalized;
				}

				// add to horizontal target
				horizontalTarget += targetDirection.normalized * horizontalDistance;
			}

			Vector3 aimDirection = Vector3.zero;

			// forward/backward aim
			if (Mathf.Abs (rsv) >= axisThreshold) {
				aimDirection = 
					(aimRange == StickRange.FOURWAY ? Vector3.zero : aimDirection) +
				SnapToWorldAxes (attachedCamera.cam.transform.forward) *
				(aimRange == StickRange.ANALOG360 ? rsv : Mathf.Sign (rsv));
			}

			// left/right aim
			if (Mathf.Abs (rsh) >= axisThreshold) {
				aimDirection = 
					(aimRange == StickRange.FOURWAY ? Vector3.zero : aimDirection) +
				SnapToWorldAxes (attachedCamera.cam.transform.right) *
				(aimRange == StickRange.ANALOG360 ? rsh : Mathf.Sign (rsh));
			}

			// valid aim direction
			if (aimDirection != Vector3.zero) {

				// update rotation forward
				rotationForward = aimDirection.normalized;
			}

			// rotate camera left
			if (crl) {
				attachedCamera.Rotate (-1);
			}

			// rotate camera right
			else if (crr) {
				attachedCamera.Rotate (1);
			}
		}
	}

	public virtual void Update(){

		// rotate weapon with model
		if (weapon != null) {
			weapon.transform.rotation = model.transform.rotation;
		}

		// debug draw line to horizontal target
		if (debug) {
			Debug.DrawLine (
				transform.position,
				horizontalTarget,
				Color.red,
				Time.deltaTime);
		}

		// jelly animations
		if (jelly) {

			bool fall = false;

			// fall
			if (!grounded && body.velocity.y <= fallVelocityThreshold) {		
				fall = true;
				modelScaleTarget = Vector3.one +
					new Vector3 (fallSquash, fallStretch, fallSquash);
			}

			// land
			else if (grounded && !idle) {
				fall = true;
				modelScaleTarget = Vector3.one;
			}

			// start idle
			if (grounded && !idle && idleDelayWait >= idleDelay) {
				idle = true;
				idleState = true;
				idleDelayWait = idleDelay + idleTime;
			}

			// flip idle state
			if (grounded && idle && idleDelayWait >= idleDelay + idleTime) {

				// reset idle delay wait
				idleDelayWait = idleDelay;

				// flip idle state
				idleState = !idleState;

				// idle in
				if (!idleState) {
					modelScaleTarget = Vector3.one +
						new Vector3 (idleSquash, idleStretch, idleSquash);
				}

				// idle out
				else {
					modelScaleTarget = Vector3.one;
				}
			}

			// apply to model scale
			model.transform.localScale = Vector3.SmoothDamp (
				model.transform.localScale,
				modelScaleTarget,
				ref modelScaleVelocity,
				idle ? idleTime : fall ? fallTime : jumpTime,
				float.MaxValue,
				Time.deltaTime);
		}

		// rotate to target
		model.transform.rotation = Quaternion.Euler (
			model.transform.eulerAngles.x,
			Mathf.SmoothDampAngle (
				model.transform.eulerAngles.y,
				Quaternion.LookRotation (rotationForward, Vector3.up).eulerAngles.y,
				ref rotationVelocity,
				rotationTime,
				float.MaxValue,
				Time.deltaTime),
			model.transform.eulerAngles.z);
	}

	public override void FixedUpdate(){
		base.FixedUpdate ();

		// add force towards horizontal target
		if (!moveCooldown.cooldown) {
			body.AddForce (
				(horizontalTarget - new Vector3 (transform.position.x, 0f, transform.position.z)).normalized *
				(Vector3.Distance (horizontalTarget, new Vector3 (transform.position.x, 0f, transform.position.z)) / horizontalDistance) *
				horizontalForce,
				ForceMode.Force);
		}

		// apply jump
		if (jump) {

			Audio.Play (jumpSound,"sfx", 0.75f * Audio.Attenuate(transform.position));

			// only jump once
			jump = false;

			// clear existing vertical velocity
			body.velocity = new Vector3 (
				body.velocity.x,
				0f,
				body.velocity.z);

			// add jump force
			body.AddForce (
				Vector3.up * (sprint ? jumpForceSprint : jumpForce),
				ForceMode.Impulse);

			// reset fall duration
			fallDuration = 0f;

			// start jump jelly animation
			if (jelly) {
				modelScaleTarget = Vector3.one +
					new Vector3 (jumpSquash, jumpStretch, jumpSquash);
			}

			// start jump cooldown
			jumpCooldown.StartCooldown(sprint ? jumpCooldownTimeSprint : jumpCooldownTime);

			// reset idle
			idle = false;
			idleState = false;
			idleDelayWait = 0f;
		}

		// store previous grounded
		bool previousGrounded = grounded;

		// reset grounded
		grounded = false;

		// reset moving block
		if (attachedMovingBlock != null) {
			attachedMovingBlock.players.Remove (this);
			attachedMovingBlock = null;
		}

		// check grounded if not jumping
		if (!jumpCooldown.cooldown) {
			foreach (Vector3 sensorPosition in sensorPositions) {

				// raycast to ground
				RaycastHit hit;
				if (Physics.SphereCast (
					    transform.TransformPoint (sensorPosition),
						sensorThickness,
					    Vector3.down,
					    out hit,
					    sensorExtends)) {

					// spawn damage
					if (firstSpawn && spawnDamage > 0) {
						firstSpawn = false;

						if (!firstGrounded) {
							Audio.Play (hammerDown2, "sfx", 0.75f * Audio.Attenuate(transform.position));
							Audio.Play (hardLandSound, "sfx", 1f * Audio.Attenuate(transform.position));
							Audio.Play (hardLandSound, "sfx", 1f * Audio.Attenuate(transform.position));
							Audio.Play (hardLandSound, "sfx", 1f * Audio.Attenuate(transform.position));

							// rumble
							if (inputEnabled) {
								StartCoroutine(xInput.Rumble (playerId, 1000f, 0.25f));
							}

							GameObject spawnSmash = (GameObject)GameObject.Instantiate (Resources.Load("Prefabs/Particles/SpawnSmash"));
							spawnSmash.transform.position = transform.position;
						}

						// apply damage to surrounding enemies
						Collider[] spawnHits = Physics.OverlapSphere (
							                       transform.position,
							                       spawnDamageRadius);
						foreach (Collider spawnHit in spawnHits) {
							if (spawnHit.attachedRigidbody != null &&
								spawnHit.attachedRigidbody.GetComponent<Enemy> () != null && // is enemy
								spawnHit.attachedRigidbody.gameObject != gameObject) { // not self

								// apply damage
								spawnHit.attachedRigidbody.GetComponent<PlayerHealth> ().health -= spawnDamage;
							}
						}
					}

					// set grounded
					grounded = true;

					// play land sound
					if (grounded && !previousGrounded && !firstGrounded) {
						Audio.Play (landSound, "sfx", 0.25f * Audio.Attenuate(transform.position));
					}

					firstGrounded = false;

					// set last grounded y position
					lastGroundedY = transform.position.y;

					// reset horizontal target
					horizontalTarget = new Vector3 (
						snapToGrid ? Mathf.Round (transform.position.x) : transform.position.x,
						0f,
						snapToGrid ? Mathf.Round (transform.position.z) : transform.position.z);

					// on a moving block
					if (hit.collider.gameObject.GetComponent<MovingBlock> () != null) {

						// attach player to moving block
						attachedMovingBlock = hit.collider.gameObject.GetComponent<MovingBlock> ();
						if (!attachedMovingBlock.players.Contains (this)) {
							attachedMovingBlock.players.Add (this);
						}

						// override horizontal target to match moving block
						//if (snapToGrid) {
							horizontalTarget = new Vector3 (
								attachedMovingBlock.transform.position.x,
								0f,
								attachedMovingBlock.transform.position.z);
						//}
					}

					// set last grounded position if not trap
					if (hit.collider.gameObject.GetComponent<Trap> ()==null){
						RaycastHit checkHit;
						if (Physics.Raycast (
							hit.point + Vector3.up * lastGroundedCheckHeight,
							Vector3.down,
							out checkHit,
							lastGroundedCheckDistance) &&
							checkHit.collider.gameObject.GetComponent<Trap> () == null) {
							lastGroundedPosition = new Vector3 (
								Mathf.Round (hit.point.x),
								hit.point.y,
								Mathf.Round (hit.point.z));
							lastGroundedMovingBlock = attachedMovingBlock;
						}
					}

					// screen shake on long fall
					if (fallDuration >= minTimeShake) {
						PlayerCamera.ShakeAll (
							minShake +
							((fallDuration - minTimeShake) / (maxTimeShake - minTimeShake)) *
							(maxShake - minShake),
							shakeTime,
							transform.position);

						Audio.Play (hardLandSound, "sfx", 0.75f * Audio.Attenuate(transform.position));
						Audio.Play (hardLandSound, "sfx", 0.75f * Audio.Attenuate(transform.position));
					}

					// reset fall duration
					fallDuration = 0f;

					// increment idle delay wait
					idleDelayWait += Time.fixedDeltaTime;

					break;
				}
			}
		}

		// increment fall duration
		fallDuration += Time.fixedDeltaTime;
	}

	public Vector3 SnapToWorldAxes(Vector3 direction){

		// z axis
		if (Mathf.Abs (direction.z) > Mathf.Abs (direction.x)) {
			direction = new Vector3 (0f, 0f, Mathf.Sign (direction.z)).normalized;
		}

		// x axis
		else if (Mathf.Abs (direction.x) > Mathf.Abs (direction.z)) {
			direction = new Vector3 (Mathf.Sign (direction.x), 0f, 0f).normalized;
		}

		return direction;
	}
}
