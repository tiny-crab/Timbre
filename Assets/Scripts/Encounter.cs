using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    public List<KeyValuePair<GameObject, Vector2>> enemyPrefabs = new List<KeyValuePair<GameObject, Vector2>>();

    void Awake() {
        foreach (Transform child in transform) {
            var overworldNPC = child.GetComponent<NPC>();
            if (overworldNPC != null) {
                enemyPrefabs.Add(
                    new KeyValuePair<GameObject, Vector2>(overworldNPC.gridPrefab, overworldNPC.transform.position)
                );
            }
            overworldNPC.enabled = false;
        }
    }

    public void Trigger() {
        foreach (Transform child in transform) {
            child.GetComponent<SpriteRenderer>().enabled = false;
            Destroy(child.gameObject);
        }
    }
}
