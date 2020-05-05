using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public string dialoguePrefabString;
    public NPCDialoguePrefab dialoguePrefab;
    public List<string> messageChunks;
    public Dialog dialog;
    public AudioClip dialogNoise;

    // specifies which prefab should be loaded into grid system when in combat
    public GameObject gridPrefab;

    void Awake () {
        dialog = (Dialog) GameObject.Find("DialogueDialog").GetComponent<Dialog>();
        if (dialoguePrefabString != "") {
            dialoguePrefab = NPCDialoguePrefabUtils.ToDialoguePrefab(dialoguePrefabString);
            messageChunks = dialoguePrefab.getNextMessage();
        }
    }

    void Update () {

    }

    void PlayerInteract() {
        if (dialog.complete) {
            dialog.PostToDialog(messageChunks, dialogNoise);
            messageChunks = dialoguePrefab.getNextMessage();
        } else {
            dialog.AdvanceDialog();
        }
    }
}
