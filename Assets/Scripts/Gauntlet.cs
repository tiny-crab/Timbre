using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    GridSystem gridSystem;

    void Awake () {
        gridSystem = GameObject.Find("GridSystem").GetComponent<GridSystem>();
    }

    void Start () {
        gridSystem.ActivateGrid(
            playerLocation: new Vector2(0, 0),
            activeParty: new List<GameObject>() {
                Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/GridPlayer"),
                Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Dog"),
            },
            enemiesToSpawn: GenerateRandomEnemies()
        );
    }

    // Update is called once per frame
    void Update()
    {
        if (gridSystem.allAlliesDefeated || gridSystem.allEnemiesDefeated) {
            EndGauntlet();
        }
    }

    private void EndGauntlet() {
        if (Application.isEditor) { UnityEditor.EditorApplication.isPlaying = false; }
        else { Application.Quit(); }
    }

    private List<KeyValuePair<GameObject, Vector2>> GenerateRandomEnemies() {
        var pair = new KeyValuePair<GameObject, Vector2>(
            // Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/ElkCult/ElkCultist"),
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/Goblin"),
            new Vector2(int.MaxValue, int.MaxValue)
        );
        return new List<KeyValuePair<GameObject, Vector2>>() {
            pair,
        };
    }
}
