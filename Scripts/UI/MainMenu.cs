using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public Transition menuTransition;
	public float showMenuDelay = 1.25f;

	public Transition transition;
	public float transitionOutTime = 0.6f;

	public float startDelay = 2f;
	public string bgMusic;

	private bool menuShown = false;
	private bool loading = false;

	private PlayerXInput xInput = new PlayerXInput();

	public void Start(){
		
		// init cinput for keyboard only
		cInput.Init ();
		cInput.Clear ();
		cInput.allowDuplicates = true;
		cInput.deadzone = 0f;
		cInput.gravity = float.MaxValue;
		cInput.sensitivity = float.MaxValue;
		cInput.SetKey ("start", Keys.Space);
		cInput.SetKey ("credits", Keys.Enter);
		cInput.SetKey ("quit", Keys.Escape);

		// show menu
		Invoke ("ShowMenu", showMenuDelay);

		// play music
		Audio.PlayMusic (bgMusic);
		Audio.GET.GetGroup ("music").Automate (1f, 2f);
	}

	public void ShowMenu(){
		menuShown = true;
		menuTransition.AlphaUp ();
	}

	public void Update(){

		// start
		if ((cInput.GetKeyDown ("start") || xInput.KeyDownAll(SCKey.MENU_START))
			&& menuShown && !loading) {
			loading = true;
			PlayButton ();
		}

		// credits
		if ((cInput.GetKeyDown ("credits") || xInput.KeyDownAll(SCKey.MENU_CREDITS))
			&& menuShown && !loading) {
			loading = true;
			CreditsButton ();
		}

		// quit
		if ((cInput.GetKeyDown ("quit") || xInput.KeyDownAll(SCKey.MENU_QUIT))) {
			Debug.Log ("Quitting game...");
			Application.Quit ();
		}

		xInput.UpdatePreviousStates ();
	}

	public void PlayButton(){

		// play select sound
		Audio.Play ("Select","ui");

		// fade out music
		Audio.GET.GetGroup ("music").Automate (0f, 1f);

		// hide menu
		menuTransition.AlphaDown ();

		// fade out
		transition.smoothTime = transitionOutTime;
		transition.AlphaUp ();

		Invoke ("LoadCharacterSelection", startDelay);
	}

	public void CreditsButton(){

		// play select sound
		Audio.Play ("Select","ui");

		// fade out music
		Audio.GET.GetGroup ("music").Automate (0f, 1f);

		// hide menu
		menuTransition.AlphaDown ();

		// fade out
		transition.smoothTime = transitionOutTime;
		transition.AlphaUp ();

		Invoke ("LoadCredits", startDelay);
	}

	public void LoadCharacterSelection(){
		SceneManager.LoadScene("CharacterSelection");
	}

	public void LoadCredits(){
		SceneManager.LoadScene("Credits");
	}
}
