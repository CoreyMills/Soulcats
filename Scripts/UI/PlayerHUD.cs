using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour {

	public Transition transition;
	public Image[] hearts;
	public Text soulGems;
	public Player player;

	public void Show(){

		// show hud
		transition.AlphaUp ();
	}

	public void Hide(){

		// hide hud
		transition.AlphaDown ();
	}

	public void Update(){

		// update hearts
		for (int i = 0; i < hearts.Length; i++) {
			hearts [i].enabled = i < player.GetComponent<PlayerHealth> ().health;
		}

		// update soulgem count
		soulGems.text = "x" + player.GetComponent<PlayerInventory>().soulGems;
	}
}
