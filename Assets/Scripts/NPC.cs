using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

	public string message;
	public Dialog dialog;

	void Awake () {
		dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();
	}

	void Update () {
		
	}

	void PlayerInteract() {
		dialog.textBlob.text = message;
	}
}
