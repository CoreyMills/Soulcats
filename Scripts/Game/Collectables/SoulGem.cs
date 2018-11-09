using UnityEngine;
using System.Collections;

public class SoulGem : PickUp 
{
	public int soulGemValue = 0;

	public override void OnCollisionEnter (Collision collision)
	{
		// check collided with player
		if(collision.gameObject.tag == "Player")
		{
			// increment soul gem by value
			collision.gameObject.GetComponent<PlayerInventory>().soulGems += soulGemValue;

			Audio.Play ("Jey_Audio/SFX/coin_ting_03", "sfx", 1f * Audio.Attenuate(transform.position));
		}
		
		base.OnCollisionEnter(collision);
	}
}