using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiCharacterSelection : MonoBehaviour {

	public string bgMusic;
	public CharacterSelection[] charSelection;
	public float waitingTime = 5f;

	private float timeLeft;
	private LevelLoader levelLoader;
	private bool loading = false;

	public void Awake () {

		// play music
		Audio.PlayMusic (bgMusic);
		Audio.GET.GetGroup ("music").Automate (1f, 2f);
		
		// init cinput for keyboard only
		cInput.Init ();
		cInput.Clear ();
		cInput.allowDuplicates = true;
		cInput.deadzone = 0f;
		cInput.gravity = float.MaxValue;
		cInput.sensitivity = float.MaxValue;
		cInput.SetKey ("p0Select", Keys.Space, Keys.Enter);
		cInput.SetKey ("p0Back", Keys.Escape, Keys.Backspace);
		cInput.SetKey ("p0Left", Keys.A, Keys.ArrowLeft);
		cInput.SetKey ("p0Right", Keys.D, Keys.ArrowRight);

		// init waiting time
		timeLeft = waitingTime;

		// get level loader
		levelLoader = GameObject.Find ("LevelLoader").GetComponent<LevelLoader> ();
		levelLoader.currentLevel = 0;
		Game.numberOfPlayers = 0;
		Game.playerPrefabNames.Clear ();

		// init player ids
		for (int i = 0; i < charSelection.Length; i++) {
			charSelection [i].playerId = i;
			ControllerPrompt[] prompts = charSelection [i].gameObject.GetComponentsInChildren<ControllerPrompt> ();
			foreach (ControllerPrompt prompt in prompts) {
				prompt.playerId = i;
			}
		}
	}

	public void Update () {

		// check players are ready before loading
		for (int i = 0; i < charSelection.Length; i++) {
			int charState = charSelection [i].state;

			// at least one player is ready
			if (charState == 2) {

				// ensure all others are ready or not entered
				bool allOthersReady = true;
				for (int i2 = 0; i2 < charSelection.Length; i2++) {
					int charState2 = charSelection [i2].state;
					if (charState2== 1) {
						allOthersReady = false;
					}
				}

				// everyone is ready, countdown timer
				if (allOthersReady) {
					timeLeft -= Time.deltaTime;
					if (timeLeft < 0f) {
						timeLeft = 0f;
					}
				}

				// not everyone is ready, reset timer
				else {
					timeLeft = waitingTime;
				}

				break;
			} 

			// no-one is ready, reset timer
			else if(charState == 1) {
				timeLeft = waitingTime;
			}
		}

		// update timer labels
		for (int i = 0; i < charSelection.Length; i++) {
			charSelection[i].timeLeftLabel.text = "00:0" + Mathf.Round (timeLeft);
		}

		// time is up
		if(timeLeft <= 0f && !loading){

			loading = true;

			// update number of players in game
			for (int i = 0; i < charSelection.Length; i++) {
				int charState = charSelection [i].state;

				// ready
				if (charState == 2) {
					Game.numberOfPlayers++;
					Game.playerPrefabNames.Add (charSelection [i].targetPlayer.name);
				}

				// update new state
				charSelection[i].state = 3;
			}

			Debug.Log (Game.numberOfPlayers + " players have entered the game!");

			// load the first level
			levelLoader.LoadNextLevel ();
		}
	}
}
