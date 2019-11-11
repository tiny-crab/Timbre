using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {

    Player player;
    Animator animator;

    Vector3 prevPos;

    void Awake () {
        player = this.transform.parent.GetComponent<Player>();
        animator = this.transform.parent.GetComponent<Animator>();
    }

    void Update () {
        var curPos = player.transform.position;

        if (prevPos != curPos) {
            // moving right
            if (prevPos.x < curPos.x) {
                if (prevPos.y < curPos.y) { animator.Play("NorthEast"); }
                else if (prevPos.y > curPos.y) { animator.Play("SouthEast"); }
                else { animator.Play("East"); }
            }
            // moving left
            else if (prevPos.x > curPos.x) {
                if (prevPos.y < curPos.y) { animator.Play("NorthWest"); }
                else if (prevPos.y > curPos.y) { animator.Play("SouthWest"); }
                else { animator.Play("West"); }
            }
            // moving down
            else if (prevPos.y > curPos.y) { animator.Play("South"); }
            // moving up
            else if (prevPos.y < curPos.y) { animator.Play("North"); }
        } else {
            animator.Play("Idle");
        }

        prevPos = player.transform.position;
    }

}