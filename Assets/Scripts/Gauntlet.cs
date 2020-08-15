using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gauntlet : MonoBehaviour
{
    [SerializeField]
    Camera mainCam;
    Vector3 ethreadMenuCamPosition;

    GridSystem gridSystem;
    GauntletState currentState;

    List<GameObject> partyMembers;

    List<GameObject> possibleThreadDrops;
    int afraidEnemiesDefeated = 0;
    GameObject ethreadMenu;
    GameObject townMenu;

    void Awake () {
        gridSystem = GameObject.Find("GridSystem").GetComponent<GridSystem>();
        ethreadMenu = GameObject.Find("EthreadMenu");
        townMenu = GameObject.Find("TownMenu");
    }

    void Start () {
        ethreadMenu.SetActive(false);
        ethreadMenuCamPosition = GameObject.Find("EthreadMenuPosition").transform.position;

        townMenu.SetActive(false);

        var possiblePartyMembers = new List<GameObject>() {
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/GridPlayer"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Dog"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Knight"),
            Resources.Load<GameObject>("Prefabs/Grid/AllyClasses/Woodsman"),
        };

        possibleThreadDrops = new List<GameObject> {
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/RedThreadPickup"),
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/BlueThreadPickup"),
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/GreenThreadPickup"),
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/PurpleThreadPickup"),
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/YellowThreadPickup"),
                Resources.Load<GameObject>("Prefabs/Overworld/Pickups/PinkThreadPickup"),
        };

        partyMembers = possiblePartyMembers.ManyRandomElements(2);

        currentState = new ThreadCustomization(this);
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
        var possibleElites = new List<GameObject> {
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/ElkCult/ElkDeacon"),
            Resources.Load<GameObject>("Prefabs/Grid/EnemyClasses/Undead/Decrepit Corpse"),
        };
        var pair = new KeyValuePair<GameObject, Vector2>(
            possibleElites.RandomElement(),
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
            Debug.Log($"Transitioning from {oldState.GetType().FullName} to {newState.GetType().FullName}");
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
            if (parent.gridSystem.allEnemiesDefeated()) {
                parent.ChangeState(typeof(ThreadCustomization));
            } else if (parent.gridSystem.allAlliesDefeated()) {
                parent.ChangeState(typeof(Complete));
            }
        }
        public override void EndState(GauntletState nextState) {
            parent.afraidEnemiesDefeated = parent.gridSystem.factions
                .Where(faction => faction.isHostileFaction)
                .SelectMany(faction => faction.entities)
                .Where(entity => entity.totalFearValue >= entity.fearThreshold).Count();
            parent.gridSystem.DeactivateGrid();
        }
    }

    public class ThreadCustomization : GauntletState {
        public ThreadCustomization(Gauntlet parent) : base(parent) {}

        // these exist in order to make sure the button click interrupts are "quantized" to the next update frame
        // instead of interrupting
        PollableButton continueButton;
        PollableButton returnButton;

        public override void StartState(GauntletState lastState) {
            parent.mainCam.transform.position = parent.ethreadMenuCamPosition;
            parent.ethreadMenu.SetActive(true);

            var ethreadMenu = parent.ethreadMenu.GetComponent<EthreadMenu>();

            ethreadMenu.PopulateParty(parent.partyMembers);
            continueButton = new PollableButton(ethreadMenu.continueButton);
            returnButton = new PollableButton(ethreadMenu.returnButton);

            // reward at the end of battle
            var possibleRewards = parent.ethreadMenu.GetComponent<EthreadMenu>().threadButtonGroups;
            possibleRewards.RandomElement().quantity++;

            possibleRewards.ManyRandomElements(parent.afraidEnemiesDefeated).ForEach(i => i.quantity++);
        }
        public override void UpdateState() {
            if (continueButton.isClicked) {
                ContinueEncounters();
            }
            if (returnButton.isClicked) {
                ReturnToTown();
            }
        }
        public override void EndState(GauntletState nextState) {
            parent.ethreadMenu.SetActive(false);
        }

        private void ContinueEncounters() {
            parent.ChangeState(typeof(Combat));
        }
        private void ReturnToTown() {
            parent.ChangeState(typeof(TownMenuScreen));
        }
    }

    public class TownMenuScreen : GauntletState {
        public TownMenuScreen(Gauntlet parent) : base(parent) {}

        PollableButton continueButton;

        public override void StartState(GauntletState lastState) {
            parent.townMenu.SetActive(true);
            var townMenu = parent.townMenu.GetComponent<TownMenu>();

            townMenu.PopulateParty(parent.partyMembers);
            continueButton = new PollableButton(townMenu.continueButton);
        }

        public override void UpdateState() {
            if (continueButton.isClicked) {
                parent.ChangeState(typeof(Combat));
            }
        }

        public override void EndState(GauntletState nextState) {
            parent.townMenu.SetActive(false);
        }

        private void ContinueEncounters() {
            parent.ChangeState(typeof(Combat));
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
