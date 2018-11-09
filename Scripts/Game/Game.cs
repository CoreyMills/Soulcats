using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public static int numberOfPlayers = 0;
	public static List<string> playerPrefabNames = new List<string>();

	[Header("TEST")]
	[Range(1,4)]
	public int testNumPlayers = 1;
	public string testPlayerPrefabName = "Player_Launcher";

	[Header("CAMERA")]
	public string cameraPrefabName = "Player_Camera_Default";
	public Transform cameraStartPosition;
	public float tilt = 40f;
	public float rot = 5f;
	public float zoom = -8f;
	public float introRotStart = -270f;
	public float introRotTime = 1.5f;
	public float introDuration = 3f;

	[Header("AUDIO")]
	public string bgMusic;
	public float pauseOverrideCutoff = 500f;

	[HideInInspector]
	public bool paused = false;

	private Transition pauseTransition;

	public void Awake()
	{
		// set number of players for testing
		if (numberOfPlayers == 0) {
			numberOfPlayers = testNumPlayers;
			Game.playerPrefabNames.Clear ();
			for (int i = 0; i < numberOfPlayers; i++) {
				Game.playerPrefabNames.Add (testPlayerPrefabName);
			}
		}

		// play background music for level
		Audio.PlayMusic (bgMusic);
		Audio.GET.GetGroup ("music").Automate (0.75f, 1f);

		// default camera width and height
		float cameraWidth = 1f;
		float cameraHeight = 1f;

		// set camera width
		if(numberOfPlayers > 1)
		{
			cameraWidth = 0.5f;
		}

		// set camera height
		if(numberOfPlayers > 2)
		{
			cameraHeight =0.5f;
		}

		// spawn players
		for(int i = 0; i < numberOfPlayers; i++)
		{
			// default camera position
			float cameraX = 0f;
			float cameraY = 0f;

			// set camera x
			if(i == 1|| i == 3)
			{
				cameraX = 0.5f;
			}

			// set camera y
			if(i == 0 || i == 1)
			{
				cameraY = 1f - cameraHeight;
			}

			// spawn player
			GameObject player = (GameObject) GameObject.Instantiate(Resources.Load("Prefabs/" + playerPrefabNames[i]),
				GameObject.Find("PlayerSpawnPoint" + i).transform.position, Quaternion.identity);
			player.GetComponent<Player> ().model.transform.rotation = GameObject.Find ("PlayerSpawnPoint" + i).transform.rotation;
			player.transform.parent = GameObject.Find("Players").transform;

			// spawn camera
			GameObject attachedCamera = (GameObject) GameObject.Instantiate(Resources.Load("Prefabs/Cameras/" + cameraPrefabName),
				cameraStartPosition.position,
				Quaternion.identity);
			attachedCamera.transform.parent = GameObject.Find("Cameras").transform;

			// change the rect info of the camera
			Camera[] cams = attachedCamera.GetComponentsInChildren<Camera>();
			foreach (Camera cam in cams) {				
				cam.rect = new Rect (cameraX, cameraY, cameraWidth, cameraHeight);
			}

			// attach player to camera
			attachedCamera.GetComponentInChildren<PlayerCamera>().player = player.GetComponent<Player>();
			attachedCamera.GetComponentInChildren<PlayerCamera> ().hud.player = player.GetComponent<Player> ();

			// set hud scale
			attachedCamera.GetComponentInChildren<PlayerCamera> ().hud.transform.localScale = 
				new Vector3 (
					cameraHeight,
					cameraHeight,
					1f);
			attachedCamera.GetComponentInChildren<PlayerCamera> ().counter.transform.localScale = 
				new Vector3 (
					cameraHeight,
					cameraHeight,
					1f);

			// attach camera to player
			player.GetComponent<Player>().attachedCamera = attachedCamera.GetComponentInChildren<PlayerCamera>();

			// set player id
			player.GetComponent<Player>().playerId = i;

			// initially disable input
			player.GetComponent<Player>().inputEnabled = false;

			// set camera tilt
			attachedCamera.GetComponentInChildren<PlayerCamera>().introRotationY = introRotStart;
			attachedCamera.GetComponentInChildren<PlayerCamera>().introRotationYTarget = rot;
			attachedCamera.GetComponentInChildren<PlayerCamera>().introRotationTime = introRotTime;
			attachedCamera.GetComponentInChildren<PlayerCamera>().introDurationTime = introDuration;
			attachedCamera.transform.Find("Player_Camera").Find ("Tilt").gameObject.transform.localRotation = Quaternion.Euler (
				tilt,
				introRotStart, 
				0f);

			// set camera zoom
			attachedCamera.GetComponentInChildren<Camera> ().transform.localPosition = 
				new Vector3 (0f, 0f, zoom);

			StartCoroutine (attachedCamera.GetComponentInChildren<PlayerCamera> ().IntroComplete (introDuration));
		}

		// add pause menu
		GameObject pause = (GameObject) GameObject.Instantiate((GameObject) Resources.Load("Prefabs/Cameras/Pause_Camera"), Vector3.zero, Quaternion.identity);
		pause.transform.parent = GameObject.Find ("Cameras").transform;
		pauseTransition = pause.GetComponentInChildren<Transition> ();
	}

	public void Pause(){

		// flip pause
		paused = !paused;

		//Debug.Log ("PAUSED: " + paused);

		// toggle pause menu
		if (paused) {
			pauseTransition.AlphaUp ();
		} else {
			pauseTransition.AlphaDown ();
		}

		// set pause audio cutoff
		Audio.GET.overrideCutoff = paused ? pauseOverrideCutoff : -1f;

		// play sound
		if (paused) {
			Audio.Play ("Collide", "ui");
		}

		// freeze time on pause
		Time.timeScale = paused ? 0f : 1f;
	}
}
