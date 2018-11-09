using UnityEngine;
using System.Collections;

public class AudioGroup : MonoBehaviour {

	public string groupName = "sfx";

	[Range(0f,1f)]
	public float volume = 1f;
	public bool automate = false;

	private float targetVolume = 1f;
	private float targetVolumeTime = 0f;
	private float targetVolumeVelocity = 0f;

	public void Awake(){

		// initial target volume
		targetVolume = volume;
	}

	public void Update(){

		// automate volume
		if (automate) {
			volume = Mathf.SmoothDamp (
				volume,
				targetVolume,
				ref targetVolumeVelocity,
				targetVolumeTime,
				float.MaxValue,
				Time.deltaTime);
		}
	}

	public void Automate(float volume, float time){
		this.targetVolume = volume;
		this.targetVolumeTime = time;
	}
}
