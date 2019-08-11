using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public string message;
    public Dialog dialog;
    public AudioClip dialogNoise;

    void Awake () {
        dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();
    }

    void Update () {

    }

    void PlayerInteract() {
        dialog.PostToDialog(message, dialogNoise);
    }
}
