using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour {
	
	public GameObject[] characters;
	public GameObject mainCamera;
	public GameObject targetPlayer;
	public GameObject playersGroup;
	public GameObject selectGroup;
	public GameObject startGroup;
	public GameObject readyGroup;
	public Text timeLeftLabel;

	public int state = 0;
	public float smoothTime = 0.4f;
	public int targetPlayerIndex;
	public int playerId = -1;

	private float startGroupAlpha;
	private Vector3 characterChangeVelocity = Vector3.zero;
	private PlayerXInput xInput = new PlayerXInput();

	void Start(){
		targetPlayerIndex = 0;
		startGroupAlpha = 1f;
	}

	void Update(){

		// init start group alpha
		startGroup.GetComponent<CanvasGroup> ().alpha = startGroupAlpha;

		// get target player from index
		targetPlayer = characters [targetPlayerIndex];

		// animate camera to target player
		mainCamera.transform.position = Vector3.SmoothDamp (mainCamera.transform.position, targetPlayer.transform.position, ref characterChangeVelocity, smoothTime);

		// back to state 0 if controller disconnected
		if(playerId > 0 && !xInput.PlayerConnected(playerId)){
			state = 0;
		}

		// start screen
		if(state==0){
			StartScreen ();

			// controller enabled for this player
			if (playerId != -1) {

				// enter player
				if ((playerId == 0 && cInput.GetKeyDown ("p0Select")) ||
					xInput.KeyDown(SCKey.SELECT_ACCEPT, playerId)) {
					state = 1;
					Audio.Play ("Hammer","ui");
				} 

				// quit to menu
				else if ((playerId == 0 && cInput.GetKeyDown ("p0Back")) ||
					xInput.KeyDown(SCKey.SELECT_BACK, playerId)) {
					SceneManager.LoadScene ("Menu");
					Audio.Play ("Collide","ui");
				}
			}
		}

		// character select screen
		else if(state==1){
			SelectionScreen ();

			// back to start screen
			if ((playerId == 0 && cInput.GetKeyDown ("p0Back")) ||
				xInput.KeyDown(SCKey.SELECT_BACK, playerId)) {
				state = 0;
				Audio.Play ("Collide","ui");
			}

			// navigate to next character
			if ((playerId == 0 && cInput.GetKeyDown ("p0Right")) ||
				xInput.KeyDown(SCKey.SELECT_RIGHT, playerId)) {
				MoveRight ();
			}

			// navigate to previous character
			else if ((playerId == 0 && cInput.GetKeyDown ("p0Left")) ||
				xInput.KeyDown(SCKey.SELECT_LEFT, playerId)) {
				MoveLeft ();
			}

			// select this character
			if ((playerId == 0 && cInput.GetKeyDown ("p0Select")) ||
				xInput.KeyDown(SCKey.SELECT_ACCEPT, playerId)) {
				state = 2;
				Audio.Play ("Hammer","ui");
			}
		}

		// ready screen
		else if(state==2){
			ReadyScreen();

			// back to character select screen
			if ((playerId == 0 && cInput.GetKeyDown ("p0Back")) || 
				xInput.KeyDown(SCKey.SELECT_BACK, playerId)) {
				state = 1;
				Audio.Play ("Collide","ui");
			}
		}

		xInput.UpdatePreviousStates ();
	}

	public void MoveRight(){

		// cap at end of character array
		if (targetPlayerIndex >= characters.Length-1) {
		} 

		// next character
		else {
			targetPlayerIndex++;
			Audio.Play ("Sprint","ui");
		}

	}
	public void MoveLeft(){

		// cap at beginning of character array
		if (targetPlayerIndex <= 0) {
		} 

		// previous character
		else {
			targetPlayerIndex--;
			Audio.Play ("Sprint","ui");
		}
	}

	void SelectionScreen(){
		startGroupAlpha = 0f;
		playersGroup.SetActive(true);
		readyGroup.GetComponent<CanvasGroup> ().alpha = 0f;
		selectGroup.GetComponent<CanvasGroup> ().alpha = 1f;
	}
	void StartScreen(){
		startGroupAlpha = 1f;
		playersGroup.SetActive(false);
		targetPlayerIndex = 0;
		readyGroup.GetComponent<CanvasGroup> ().alpha = 0f;
		selectGroup.GetComponent<CanvasGroup> ().alpha = 0f;
	}
	void ReadyScreen(){
		startGroupAlpha = 0f;
		playersGroup.SetActive(false);
		readyGroup.GetComponent<CanvasGroup> ().alpha = 1f;
		selectGroup.GetComponent<CanvasGroup> ().alpha = 0f;
	}
}
