using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerThread : MonoBehaviour {

	// Let's imagine that the player is just another thread running in the game.
	// There will only be one thing they can "execute" at once, so let's force the interactions
	// between the keyboard and game to be limited to a single object.

	// this mainFocus will be assigned by the game state
	private ControllerInteractable mainFocus;

	public void Update() {
		
	}

}
