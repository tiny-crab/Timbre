using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : ControllerInteractable {

    public Text textBlob = null;
    public Image continuationArrow = null;
    private AudioSource dialogSound;
    private bool pitched = false;
    private List<string> messageChunks;
    private int messageChunkCounter;

    public bool complete = true;

    // Use this for initialization
    void Start () {
        textBlob = this.transform.GetChild(0).gameObject.GetComponent<Text>();
        dialogSound = this.GetComponent<AudioSource>();
        continuationArrow = this.transform.Find("ContinueArrow").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () {
        continuationArrow.enabled = !complete;
        if (!complete && messageChunkCounter == this.messageChunks.Count) {
            complete = true;
            messageChunkCounter = 0;
        }
    }

    public bool isCurrentDialogue(List<string> messageChunks) {
        return messageChunks.Equals(this.messageChunks);
    }

    // set up dialogue
    public void PostToDialog (List<string> messageChunks, AudioClip dialogNoise=null, bool pitched=true) {
        StopAllCoroutines();
        this.messageChunks = messageChunks;
        messageChunkCounter = 0;
        if (dialogNoise != null) { dialogSound.clip = dialogNoise; }
        this.pitched = pitched;
        complete = false;

        StartCoroutine(TypeOut(this.messageChunks[messageChunkCounter], dialogSound.clip, pitched));
        messageChunkCounter++;
    }

    public void PostToDialog (string message, AudioClip dialogNoise=null, bool pitched=true) {
        PostToDialog(new List<string> { message }, dialogNoise, pitched);
    }

    // player continues dialogue
    public void AdvanceDialog() {
        if (complete) { return; }

        StartCoroutine(TypeOut(this.messageChunks[messageChunkCounter], dialogSound.clip, pitched));
        messageChunkCounter++;
    }

    IEnumerator TypeOut (string message, AudioClip dialogNoise, bool pitched) {
        this.textBlob.text = "";
        var punctuationWait = 0.1f;
        var otherWait = .025f;

        foreach (char letter in message.ToCharArray()) {
            this.textBlob.text += letter;
            if (pitched) { dialogSound.pitch = (float) Mathf.Sqrt(((int) letter * .03f)); }
            dialogSound.PlayOneShot(dialogSound.clip);
            if (new List<char>() {',', '.', '!', '?'}.Contains(letter)) {
                yield return new WaitForSeconds(punctuationWait);
            }
            else {
                yield return new WaitForSeconds(otherWait);
            }

        }
    }
}
