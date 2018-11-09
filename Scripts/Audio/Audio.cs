using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Audio : MonoBehaviour {

	public const float DEFAULT_CUTOFF = 22000f;
	public static Audio GET;

	public string audioPath = "Audio/";
	public float overrideCutoff =-1f;
	public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
	public float audioDistanceMax = 15f;
	
	private Dictionary<string,AudioGroup> groups = new Dictionary<string, AudioGroup> ();
	private Dictionary<string,AudioClip> cache = new Dictionary<string, AudioClip>();
	private List<GameObject> pool = new List<GameObject>();
	private Dictionary<int, GameObject> currentSounds = new Dictionary<int, GameObject>();
	private int currentId = 0;

	public AudioGroup GetGroup(string groupName){
		return groups [groupName];
	}

	private void Awake(){
		GET = this;

		// populate groups
		AudioGroup[] groups = gameObject.GetComponents<AudioGroup> ();
		foreach (AudioGroup g in groups) {
			this.groups.Add(g.groupName, g);
		}
	}

	private void Update(){

		// update current sounds
		GameObject[] currentSoundsList = currentSounds.Values.ToArray ();
		foreach (GameObject g in currentSoundsList) {
			AudioSound a = g.GetComponent<AudioSound>();
			AudioSource s = g.GetComponent<AudioSource>();
			s.volume = a.volume * groups[a.groupName].volume;
			s.pitch = a.pitch * (a.pitchAffectedByTimeScale ? Time.timeScale : 1f);
			g.GetComponent<AudioLowPassFilter>().cutoffFrequency = overrideCutoff != -1f ? overrideCutoff : a.lowpass;
		}
	}
	
	public static int Play(
		string path,  
		string groupName = "sfx",
		float volume =1f, 
		float lowPass=DEFAULT_CUTOFF, 
		float minPitch=0.9f, 
		float maxPitch =1.1f){	

		int id = GET.GetNextId ();

		if (path.Trim ().Length > 0) {
			GET.StartCoroutine (
				GET.PlaySound (
					path, 
					volume, 
					lowPass, 
					Random.Range (minPitch, maxPitch), 
					groupName, 
					false,
					GET.reverbPreset,
					id,
					false));
		}

		return id;
	}	
	
	public static int PlayLoop(
		string path,
		string groupName = "sfx",
		float volume =1f, 
		float lowPass=DEFAULT_CUTOFF, 
		float minPitch=0.9f, 
		float maxPitch =1.1f){
		int id = GET.GetNextId ();
		GET.StartCoroutine(
			GET.PlaySound (
				path, 
				volume, 
				lowPass, 
				Random.Range(minPitch,maxPitch), 
				groupName, 
				true,
				GET.reverbPreset,
				id,
				false));
		return id;
	}

	public static int PlayMusic(
		string path, 
		float volume =1f, 
		string groupName = "music"){
		int id = GET.GetNextId ();
		GET.StartCoroutine(
			GET.PlaySound (
				path, 
				volume, 
				DEFAULT_CUTOFF, 
				1f, 
				groupName, 
				true,
				AudioReverbPreset.Off,
				id,
				false));
		return id;
	}

	public IEnumerator PlaySound(
		string path, 
		float volume, 
		float lowpass, 
		float pitch, 
		string groupName, 
		bool loop,
		AudioReverbPreset reverb,
		int id,
		bool affectedByTimeScale){

		// add to cache
		if(!cache.ContainsKey(audioPath + path)){
			AudioClip clip = (AudioClip) Resources.Load(audioPath+path);
			cache.Add(audioPath+path, clip);
		}

		// get clip
		AudioClip clipToPlay = cache [audioPath + path];

		// fetch from pool
		GameObject fetched = FetchFromPool(groupName);

		// set audio params
		fetched.GetComponent<AudioSound> ().groupName = groupName;
		fetched.GetComponent<AudioSound> ().volume = volume;
		fetched.GetComponent<AudioSound> ().pitch = pitch;
		fetched.GetComponent<AudioSound> ().lowpass = lowpass;
		fetched.GetComponent<AudioSound> ().pitchAffectedByTimeScale = affectedByTimeScale;

		// apply filters
		fetched.GetComponent<AudioLowPassFilter> ().cutoffFrequency = overrideCutoff != -1f ? overrideCutoff : lowpass;
		fetched.GetComponent<AudioReverbFilter> ().reverbPreset = reverb;

		// add to current sounds
		currentSounds.Add (id, fetched);

		// one shot
		if (!loop) {
			iTween.Stab (fetched, iTween.Hash (
				"audioclip", clipToPlay,
				"volume", volume * groups [groupName].volume,
				"pitch", pitch * (affectedByTimeScale ? Time.timeScale : 1f)));

			// return to pool
			yield return new WaitForSeconds (clipToPlay.length / (pitch * (affectedByTimeScale ? Time.timeScale : 1f)));
			pool.Add (fetched);
			currentSounds.Remove(id);
		}

		// looping
		else {
			fetched.GetComponent<AudioSource>().clip = clipToPlay;
			fetched.GetComponent<AudioSource>().volume = volume * groups[groupName].volume;
			fetched.GetComponent<AudioSource>().pitch = pitch * (affectedByTimeScale ? Time.timeScale : 1f);
			fetched.GetComponent<AudioSource>().loop = loop;
			fetched.GetComponent<AudioSource>().Play();
		}
	}

	public static void Stop(int id){
		GET.StopSound (id);
	}

	public void StopSound(int id){
		if (currentSounds.ContainsKey (id)) {
			GameObject sound = currentSounds [id];
			sound.GetComponent<AudioSource> ().Stop ();
			pool.Add (sound);
			currentSounds.Remove (id);
		}
	}

	private GameObject FetchFromPool(string groupName){

		// fetch existing
		if (pool.Count > 0) {
			GameObject fetched = pool [0];
			fetched.name = groupName;
			pool.RemoveAt (0);
			return fetched;
		}

		// fetch new
		else {
			GameObject audioClip = new GameObject(groupName);
			audioClip.transform.parent = transform;
			audioClip.AddComponent<AudioSound>();
			audioClip.AddComponent<AudioSource>();
			audioClip.AddComponent<AudioLowPassFilter>().cutoffFrequency = DEFAULT_CUTOFF;
			audioClip.AddComponent<AudioReverbFilter> ().reverbPreset = reverbPreset;
			return audioClip;
		}
	}

	public int GetNextId(){
		currentId++;
		return currentId;
	}

	public static float Attenuate(Vector3 position){

		// get closest camera target distance
		float closestDistance = float.MaxValue;
		PlayerCamera[] cameras = GameObject.Find("Cameras").GetComponentsInChildren<PlayerCamera>();
		for(int j = 0; j< cameras.Length; j++)
		{
			// calculate distance from camera target
			Transform cameraTarget = cameras[j].temporaryTarget != null ? cameras[j].temporaryTarget : cameras[j].player.transform;
			float distance = Vector3.Distance(position, cameraTarget.position);
			if (distance < closestDistance) {
				closestDistance = distance;
			}
		}

		// calculate attenuation of volume
		return 1f - closestDistance/Audio.GET.audioDistanceMax;
	}
}
