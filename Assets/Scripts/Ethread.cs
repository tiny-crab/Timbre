using System;
using UnityEngine;

public class Ethread : MonoBehaviour {

    public EthreadEffect effect;
    public string effectName;

    public void Awake () {
        if (effectName == "RedThread") {
            effect = new RedEthreadEffect();
        }
        else if (effectName == "BlueThread") {
            effect = new BlueEthreadEffect();
        }
        else if (effectName == "GreenThread") {
            effect = new GreenEthreadEffect();
        }
    }

}