using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Credits : MonoBehaviour {

	public Transition creditsTransition;
	public float showCreditsDelay = 1.25f;

	public Transition rollTransition;
	public float showRollDelay = 2.5f;

	public Transition transition;
	public float transitionOutTime = 0.6f;

	public float startDelay = 2f;
	public string bgMusic;

	private bool creditsShown = false;
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
		cInput.SetKey ("return", Keys.Space, Keys.Escape);

		// show credits
		Invoke ("ShowCredits", showCreditsDelay);

		// show roll
		Invoke ("ShowRoll", showRollDelay);

		// play music
		Audio.PlayMusic (bgMusic);
		Audio.GET.GetGroup ("music").Automate (1f, 2f);
	}

	public void ShowCredits(){
		creditsShown = true;
		creditsTransition.AlphaUp ();
	}

	public void ShowRoll(){
		rollTransition.AlphaUp ();
	}

	public void Update(){

		// return
		if ((cInput.GetKeyDown ("return") || xInput.KeyDownAll(SCKey.MENU_START) || xInput.KeyDownAll(SCKey.MENU_QUIT))
			&& creditsShown && !loading) {
			loading = true;
			ReturnButton ();
		}

		xInput.UpdatePreviousStates ();
	}

	public void ReturnButton(){

		// play select sound
		Audio.Play ("Select","ui");

		// fade out music
		Audio.GET.GetGroup ("music").Automate (0f, 1f);

		// hide menu
		creditsTransition.AlphaDown ();

		// fade out
		transition.smoothTime = transitionOutTime;
		transition.AlphaUp ();

		Invoke ("LoadMenu", startDelay);
	}

	public void LoadMenu(){
		SceneManager.LoadScene("Menu");
	}
}
