using System;
using System.Collections.Generic;
using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    [SerializeField]
    Camera mainCam;
    Vector3 ethreadMenuCamPosition;

    GridSystem gridSystem;
    GauntletState currentState;

    List<GameObject> partyMembers;

    GameObject ethreadMenu;

    void Awake () {
        gridSystem = GameObject.Find("GridSystem").GetComponent<GridSystem>();
        ethreadMenu = GameObject.Find("EthreadMenu");
    }

    void Start () {
        ethreadMenu.SetActive(false);
        ethreadMenuCamPosition = GameObject.Find("EthreadMenuPosition").transform.position;

        var possiblePartyMembers = new List<GameObject>() {
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/GridPlayer"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Dog"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Knight"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Woodsman"),
        };

        partyMembers = possiblePartyMembers.ManyRandomElements(2);

        currentState = new Combat(this);
        currentState.StartState(null);
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
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/Undead/Decrepit Corpse"),
            new Vector2(int.MaxValue, int.MaxValue)
        );
        return new List<KeyValuePair<GameObject, Vector2>>() {
            pair,
        };
    }

    private void ChangeState(Type newStateType) {
        if (newStateType.IsSubclassOf(typeof(GauntletState))) {
            var oldState = currentState;
            var newState = (GauntletState) Activator.CreateInstance(newStateType, new object[] { this });
            oldState.EndState(newState);
            newState.StartState(oldState);
            currentState = newState;
        }
    }

    public class GauntletState {
        public Gauntlet parent;
        public GauntletState(Gauntlet parent) { this.parent = parent; }

        public virtual void StartState(GauntletState lastState) {}
        public virtual void UpdateState() {}
        public virtual void EndState(GauntletState nextState) {}
    }

    public class Combat : GauntletState {
        public Combat(Gauntlet parent) : base(parent) {}

        public override void StartState(GauntletState lastState) {
            parent.mainCam.transform.position = new Vector3(0, 0, -10);

            parent.gridSystem.ActivateGrid(
                playerLocation: new Vector2(0, 0),
                activeParty: parent.partyMembers,
                enemiesToSpawn: parent.GenerateRandomEnemies()
            );
        }
        public override void UpdateState() {
            if (parent.gridSystem.allAlliesDefeated || parent.gridSystem.allEnemiesDefeated) {
                parent.ChangeState(typeof(ThreadCustomization));
            }
        }
        public override void EndState(GauntletState nextState) {
            parent.gridSystem.DeactivateGrid();
        }
    }

    public class ThreadCustomization : GauntletState {
        public ThreadCustomization(Gauntlet parent) : base(parent) {}

        public override void StartState(GauntletState lastState) {
            parent.mainCam.transform.position = parent.ethreadMenuCamPosition;
            parent.ethreadMenu.SetActive(true);
            parent.ethreadMenu.GetComponent<EthreadMenu>().PopulateParty(parent.partyMembers);
        }
        public override void UpdateState() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                parent.ChangeState(typeof(Combat));
            }
        }
        public override void EndState(GauntletState nextState) {
            parent.ethreadMenu.SetActive(false);
        }
    }

    public class Complete : GauntletState {
        public Complete(Gauntlet parent) : base(parent) {}

        public override void StartState(GauntletState lastState) {
            parent.EndGauntlet();
        }
        public override void UpdateState() {}
        public override void EndState(GauntletState nextState) {}
    }
}
