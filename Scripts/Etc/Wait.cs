using UnityEngine;
using System.Collections;

public class Wait : MonoBehaviour {

	public static IEnumerator ForRealSeconds(float delay){

		// wait for real seconds
		float waitTime = Time.realtimeSinceStartup + delay;
		while (Time.realtimeSinceStartup < waitTime){
			yield return null;
		}
	}
}
