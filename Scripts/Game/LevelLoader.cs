using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour {
	
	public int currentLevel = 0;	
	public string[] levelList;
	public float loadDelay = 2f;
	public Transition transition;
	public float transitionOutTime = 0.6f;
	public float transitionInTime = 1.2f;

	public void Start () {

		// always retain level loader
		DontDestroyOnLoad (this.gameObject);
	}

	public void LoadNextLevel(){

		// load next level
		StartCoroutine(Load(levelList[currentLevel]));

		// increment next level
		currentLevel++;
	}

	public IEnumerator Load(string levelName){

		// fade out music
		Audio.GET.GetGroup ("music").Automate (0f, 1f);

		// fade out
		transition.smoothTime = transitionOutTime;
		transition.AlphaUp ();

		yield return new WaitForSeconds (loadDelay);

		// load next level
		SceneManager.LoadScene(levelName);

		// fade out
		transition.smoothTime = transitionInTime;
		transition.AlphaDown ();
	}
}
