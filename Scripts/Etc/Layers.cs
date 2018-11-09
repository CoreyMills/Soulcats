using UnityEngine;
using System.Collections;

public class Layers {

	public static bool LayerInMask(int layer, LayerMask mask){

		// layer is in layer mask
		return ((1 << layer) & mask) != 0;
	}
}
