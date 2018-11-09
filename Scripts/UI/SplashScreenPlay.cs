using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SplashScreenPlay : MonoBehaviour {

	public float delay = 9f;

	void Start () {

		// hide cursor
		Cursor.visible = false;

		// play ident video
		RawImage video = GetComponent<RawImage>();
		MovieTexture videoTexture = (MovieTexture)video.mainTexture;
		videoTexture.Play();

		Invoke ("LoadMenu", delay);
	}

	void LoadMenu(){
		SceneManager.LoadScene("Menu");
	}
}
