using System;
using UnityEngine;

public class Ethread : MonoBehaviour {

    public EthreadEffect effect;
    public string effectName;

    public void Awake () {
        if (effectName == "RedThread") {
            effect = new DamageEthreadEffect();
        }
        else if (effectName == "BlueThread") {
            effect = new MoveEthreadEffect();
        }
        else if (effectName == "GreenThread") {
            effect = new SPEthreadEffect();
        }
        else if (effectName == "PurpleThread") {
            effect = new PrimarySkillEthreadEffect();
        }
        else if (effectName == "YellowThread") {
            effect = new SecondarySkillEthreadEffect();
        }
        else if (effectName == "PinkThread") {
            effect = new TertiarySkillEthreadEffect();
        }
    }

}