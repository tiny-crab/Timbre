using System;
using System.Collections.Generic;
using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    GridSystem gridSystem;
    GauntletState currentState;

    GameObject ethreadMenu;

    void Awake () {
        gridSystem = GameObject.Find("GridSystem").GetComponent<GridSystem>();
        ethreadMenu = GameObject.Find("EthreadMenu");
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

        ethreadMenu.SetActive(false);

        currentState = new Combat(this);
    }

    void Update() {
        currentState.UpdateState();
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

    private void ChangeState(Type newStateType) {
        if (newStateType.IsSubclassOf(typeof(GauntletState))) {
            var oldState = currentState;
            oldState.EndState();
            var newState = (GauntletState) Activator.CreateInstance(newStateType, new object[] { this });
            newState.StartState();
            currentState = newState;
        }
    }

    public class GauntletState {
        public Gauntlet parent;
        public GauntletState(Gauntlet parent) { this.parent = parent; }

        public virtual void StartState() {}
        public virtual void UpdateState() {}
        public virtual void EndState() {}
    }

    public class Combat : GauntletState {
        public Combat(Gauntlet parent) : base(parent) {}

        public override void StartState() {}
        public override void UpdateState() {
            if (parent.gridSystem.allAlliesDefeated || parent.gridSystem.allEnemiesDefeated) {
                parent.ChangeState(typeof(ThreadCustomization));
            }
        }
        public override void EndState() {}
    }

    public class ThreadCustomization : GauntletState {
        public ThreadCustomization(Gauntlet parent) : base(parent) {}

        public override void StartState() {
            parent.ethreadMenu.SetActive(true);
        }
        public override void UpdateState() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                parent.ChangeState(typeof(Complete));
            }
        }
        public override void EndState() {
            parent.ethreadMenu.SetActive(false);
        }
    }

    public class Complete : GauntletState {
        public Complete(Gauntlet parent) : base(parent) {}

        public override void StartState() {
            parent.EndGauntlet();
        }
        public override void UpdateState() {}
        public override void EndState() {}
    }
}
