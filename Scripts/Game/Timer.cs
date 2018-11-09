using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Timer : MonoBehaviour 
{
	public float countdownSeconds = 300f;

	void Update () 
	{
		// decrement countdown
		countdownSeconds -= Time.deltaTime;

		// countdown ended
		if (countdownSeconds <= 0f) {
		}
	}
}
