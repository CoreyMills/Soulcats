using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControllerPrompt : MonoBehaviour {

	[TextArea(2,2)]
	public string alternate;
	public Image buttonImage;
	public int playerId = -1;

	private string defaultLabel;
	private PlayerXInput xInput = new PlayerXInput();

	public void Start(){
		defaultLabel = GetComponent<Text> ().text;
	}

	public void Update(){

		// show default label
		if ((playerId == -1f && xInput.GamePadsConnected ()) ||
		   (playerId != -1f && xInput.PlayerConnected (playerId))) {
			GetComponent<Text> ().text = defaultLabel;
			if (buttonImage != null) {
				buttonImage.enabled = true;
			}
		}

		// show alternate message
		if (!xInput.GamePadsConnected() ||
			(playerId != -1f && !xInput.PlayerConnected(playerId))) {
			GetComponent<Text> ().text = alternate;
			if (buttonImage != null) {
				buttonImage.enabled = false;
			}
		}

		xInput.UpdatePreviousStates ();
	}
}
