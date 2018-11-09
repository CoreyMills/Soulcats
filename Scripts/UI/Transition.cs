using UnityEngine;
using System.Collections;

public class Transition : MonoBehaviour {	

	public float smoothTime=1.5f;
	public float transitionAlpha = 0f;
	public float transitionAlphaStart = 1f;
	public bool ignoreTimeScale = false;

	private float transitionVelocity= 0f;

	void Start(){

		// initial alpha
		gameObject.GetComponent<CanvasGroup> ().alpha = transitionAlphaStart;
	}

	void Update () {

		// smoothly damp alpha to transitionAlpha
		gameObject.GetComponent<CanvasGroup> ().alpha = 
			Mathf.SmoothDamp (
				gameObject.GetComponent<CanvasGroup> ().alpha, 
				transitionAlpha, 
				ref transitionVelocity, 
				smoothTime, 
				float.MaxValue, 
				ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
	}

	public void AlphaDown(){
		transitionAlpha = 0f;
	}

	public void AlphaUp(){
		transitionAlpha = 1f;
	}
}
