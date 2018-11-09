using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

	public GameObject shakeContainer;
	public Camera cam;
	public Player player;
	public PlayerHUD hud;
	public CounterHUD counter;
	public float panTime = 0.25f;
	public float rotationTime = 0.25f;
	public float maxShakeDistance= 10f;
	public Transform temporaryTarget = null;
	public float introRotationY;
	public float introRotationYTarget;
	public float introRotationTime = 0f;
	public float introDurationTime = 0f;

	private Vector3 panVelocity;
	private float rotationTarget = 0f;
	private float rotationVelocity = 0f;
	private float currentShakeScore = 0f;
	private GameObject intro;
	private float introRotationVelocity = 0f;
	private float storedPanTime = 0f;
	private float storedRotationTime = 0f;
	private float panTimeVelocity;

	private PlayerXInput xInput = new PlayerXInput();

	public void DeathRumble(){
		
		// rumble
		StartCoroutine(xInput.Rumble (player.playerId, 1000f, 0.2f));
	}

	public void Start(){

		// store pan and rotation times
		storedPanTime = panTime;
		storedRotationTime = rotationTime;

		// get intro object
		intro = GetComponentInChildren<Spinner> ().gameObject;

		// set intro and rotation times
		panTime = introRotationTime;
		rotationTime = 0f;
	}

	public IEnumerator IntroComplete(float delay){

		yield return new WaitForSeconds (delay);

		// revert rotation times
		rotationTime = storedRotationTime;

		// enable player input
		player.inputEnabled = true;

		// enable huds
		hud.Show();
		counter.Show ();
	}

	public void Update(){

		// smooth pantime
		panTime = Mathf.SmoothDamp (
			panTime,
			storedPanTime,
			ref panTimeVelocity,
			introDurationTime,
			float.MaxValue,
			Time.deltaTime);

		// pan to target
		transform.position = Vector3.SmoothDamp (
			transform.position,
			temporaryTarget == null ?
				new Vector3(player.transform.position.x, player.lastGroundedY, player.transform.position.z) :
				temporaryTarget.position,
			ref panVelocity,
			panTime,
			float.MaxValue,
			Time.deltaTime);

		// rotate to target
		Quaternion newRot = Quaternion.Euler (
			0f,
			Mathf.SmoothDampAngle (
				transform.rotation.eulerAngles.y,
				rotationTarget,
				ref rotationVelocity,
				rotationTime,
				float.MaxValue,
				Time.deltaTime),
			0f);
		if (!float.IsNaN (newRot.x)) {
			transform.rotation = newRot;
		}

		// rotate intro
		introRotationY =Mathf.SmoothDamp (
			introRotationY,
			introRotationYTarget,
			ref introRotationVelocity,
			introRotationTime,
			float.MaxValue,
			Time.deltaTime);
		intro.transform.localRotation = 
			Quaternion.Euler (
			intro.transform.localRotation.eulerAngles.x,
			introRotationY,
			intro.transform.localRotation.eulerAngles.z);				
	}

	public void Rotate(int direction){

		// add to rotation target
		rotationTarget += direction * 90f;

		Audio.Play ("Jey_Audio/Player/player_sneak_out_01","sfx", 0.2f);
	}

	public static void ShakeAll(float amount, float time, Vector3 position){

		// shake all cameras
		PlayerCamera[] camerasShake = GameObject.Find("Cameras").GetComponentsInChildren<PlayerCamera>();
		for(int j = 0; j< camerasShake.Length; j++)
		{
			camerasShake[j].Shake(amount, time, position);
		}
	}

	public void Shake(float amount, float time, Vector3 position){

		// calculate distance from camera to shake
		float distance = Vector3.Distance(position, transform.position);

		// calculate strength of shake
		float strength = amount * (1f - distance/maxShakeDistance);

		// calculate shake score
		float shakeScore = strength * time;

		// only shake if exceeds current shake score
		if (shakeScore >= currentShakeScore) {
			currentShakeScore = shakeScore;

			// apply screen shake
			Vector3 shakeAmount = Vector3.one;
			iTween.ShakePosition (
				shakeContainer,
				iTween.Hash (
					"amount", shakeAmount * strength,
					"time", time
				)
			);

			StartCoroutine (ShakeComplete (time));
		}
	}

	private IEnumerator ShakeComplete(float delay){

		// shake complete, reset score after delay
		yield return new WaitForSeconds(delay);

		currentShakeScore = 0f;
	}
}
