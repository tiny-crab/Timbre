using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : ControllerInteractable {

    public Text textBlob = null;
    private AudioSource dialogSound;

    // Use this for initialization
    void Start () {
        textBlob = this.transform.GetChild(0).gameObject.GetComponent<Text>();
        dialogSound = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {

    }

    public void PostToDialog (string message, AudioClip dialogNoise=null, bool pitched=true) {
        StopAllCoroutines();
        StartCoroutine(TypeOut(message, dialogNoise, pitched));
    }

    IEnumerator TypeOut (string message, AudioClip dialogNoise, bool pitched) {
        this.textBlob.text = "";
        var punctuationWait = 0.1f;
        var otherWait = .025f;

        if (dialogNoise != null) { dialogSound.clip = dialogNoise; }

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
