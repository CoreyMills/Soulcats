using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CounterHUD : MonoBehaviour {

	public Transition transition;
	public Text label;

	private Teleporter teleporter;

	public void Start(){

		// find teleporter
		teleporter = GameObject.FindObjectOfType<Teleporter>();
	}

	public void Show(){

		// show hud
		if (teleporter.soulGemsTarget > 0) {
			transition.AlphaUp ();
		}
	}

	public void Hide(){

		// hide hud
		transition.AlphaDown ();
	}

	public void Update(){

		// update soulgem count
		label.text = teleporter.soulGems + "/" + teleporter.soulGemsTarget;
	}
}
