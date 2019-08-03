using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : ControllerInteractable {

    public Text textBlob = null;

    // Use this for initialization
    void Start () {
        textBlob = this.transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update () {

    }

    void PostToDialog (string message) {
        this.textBlob.text = message;
    }
}
