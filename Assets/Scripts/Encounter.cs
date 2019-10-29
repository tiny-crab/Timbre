using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    void Awake() {
        foreach (Transform child in transform) {
            var overworldNPC = child.GetComponent<NPC>();
            if (overworldNPC != null) {
                enemyPrefabs.Add(overworldNPC.gridPrefab);
            }
            overworldNPC.enabled = false;
        }
    }
}
