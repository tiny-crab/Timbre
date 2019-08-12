using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour {

    public GameObject[,] initTileMap = new GameObject[8,8];
    public GameObject tile;

    // TODO UP: would be passed from a context in the overworld
    public GameObject gridEntity;
    public GameObject gridNPC;
    public GameObject gridPlayer;
    public GameObject knightPrefab;

    public System.Random rnd = new System.Random();

    // this is less important once more allies are on the field
    public GridEntity player;

    // TODO SIDE: these feel like utility functions and should be moved
    public bool waiting = false;
    // time between turns
    public float turnGapTime = 0.5f;
    // time between AI steps
    public float aiStepTime = 0.25f;
    // just wait whenever you feel like it
    public float waitTime = 0.1f;

    public CombatComponent combat = new CombatComponent();
    public TilemapComponent tilemap = new TilemapComponent();

    // TODO SIDE: Audio + UI should be in a different class
    public Dialog dialog;
    public AudioClip dialogNoise;


    void Start () {
        CreateTilemapComponent();

        knightPrefab = Resources.Load<GameObject>("Prefabs/AllyClasses/Knight");

        // TODO UP: Should just get all entities from a parent system
        // this Start() function should handle clamping their position to the nearest tile
        var npc = PutEntity(1, 1, gridNPC);
        var player = PutEntity(5, 5, gridPlayer);
        var knight = PutEntity(5, 3, knightPrefab);
        var enemyFaction = new Faction("Enemy", false, npc);
        var playerFaction = new Faction("Player", true, player, knight);

        dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();

        combat.Start(this, enemyFaction, playerFaction);
    }

    // TODO: restructure this to be a bit clearer - but this is the main FSM of the grid system
    // nothing else actually contain input and user behavior checks, so this is actually not bad...
    void Update () {

        // states:
        // WAITING
            // don't do anything
            // exit predicate:
                // wait time is complete -> go back to previous state
        // NO_SELECTION
            // Grid was just instantiated
            // exit predicates:
                // click on allied character -> ALLY_SELECTED(tile)
                // click on enemy character -> ENEMY_SELECTED(tile)
                // press end turn button || allies out of resources -> END_TURN
        // ALLY_SELECTED
            // clicked on an ally
            // exit predicates:
                // click empty tile within move range -> ALLY_MOVE(tile)
                // click on tile with enemy within attack range -> ALLY_ATTACK(tile)
                // press key to select a skill -> SKILL_ACTIVATE(skill)
        // ENEMY_SELECTED
            // clicked on an enemy
            // exit predicates:
                // selection command completes -> NO_SELECTION
        // ALLY_MOVE
            // moved an ally
            // exit predicates:
                // resolve movement (hazards, etc.) -> NO_SELECTION || ALLY_SELECTED (decide based on flow)
        // ALLY_ATTACK
            // attacked with an ally
            // exit predicates:
                // resolve attack (damage receivers, etc.) -> NO_SELECTION || ALLY_SELECTED (decide based on flow)
        // SKILL_ACTIVATE
            // clicked a button to activate a skill
            // some skills' effects happen immediately, others require different types of targets
            // exit predicates:
                // if no selection, resolve skill effect -> NO_SELECTION || ALLY_SELECTED (decide based on flow)
                // if selection -> SKILL_SELECT
        // SKILL_SELECT
            // activated a skill that requires a target
            // exit predicates:
                // press button to cancel skill -> ALLY_SELECTED
                // click target, but more targets to select -> stay in SKILL_SELECT
                // clicked all targets -> SKILL_SELECTION_RESOLVED(targets)
        // SKILL_SELECTION_RESOLVED
            // finished clicking targets for skill
            // exit predicates:
                // resolve effects -> NO_SELECTION
        // END_TURN
            // faction's turn has ended, cycle to next faction
            // exit predicates:
                // if not a player faction -> AI_TURN(faction)
                // if player faction -> NO_SELECTION
        // AI_TURN
            // run AI turn calculation and resolution
            // exit predicates:
                // AI has resolved turn -> END_TURN


        if (!waiting) {
            if (combat.currentFaction.isPlayerFaction) {
                if (Input.GetMouseButtonDown(0)) {

                    // branch here:
                    // if mouse hit a game element, pass it to Combat System
                    // if mouse hit a UI element, pass it to a (unimplemented) UI System

                    Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
                    Debug.Log(hitInfo.collider.gameObject.name);
                    Tile mouseTile = hitInfo.collider.gameObject.GetComponent<Tile>();

                    if (mouseTile != null) {
                        combat.SelectTile(mouseTile);
                        tilemap.SelectTile(mouseTile);
                        if(combat.selectedEntity == null) {
                            tilemap.ResetTileSelection(tilemap.moveRange, tilemap.attackRange);
                        }
                    }
                }
                if(Input.GetKey(KeyCode.G)) {
                    StartCoroutine(Coroutines.EndTurn);
                }
                // TODO SIDE: states should be defined in another class
                if(Input.GetKey(KeyCode.E)) {
                    if (combat.selectedEntity != null && tilemap.skillRange.tiles.Count == 0) {
                        dialog.PostToDialog("skill activated", dialogNoise, false);
                        tilemap.ActivateSkill(combat.selectedEntity);
                        StartCoroutine(Coroutines.WaitAMoment);
                    } else {
                        dialog.PostToDialog("skill deactivated", dialogNoise, false);
                        tilemap.DeactivateSkill(combat.selectedEntity);
                        StartCoroutine(Coroutines.WaitAMoment);
                    }

                    //combat.ActivateSkill(selectedEntity)
                }
            }
            else if (combat.currentFaction.isHostileFaction) {
                if (!waiting) {
                    StartCoroutine(Coroutines.ExecuteAIStep);
                }
                StartCoroutine(Coroutines.EndTurn);
            }
        }
    }

    void CreateTilemapComponent() {
        Vector3 origin = new Vector3(0, 0, 0);
        // assuming tiles are squares
        float tileWidth = tile.GetComponent<SpriteRenderer>().size.x;
        Vector2 gridSize = new Vector2(tileWidth * initTileMap.GetLength(0), tileWidth * initTileMap.GetLength(1));
        Vector3 topLeft = new Vector3(origin.x - (gridSize.x / 4), origin.y + (gridSize.y / 4), origin.z);

        for (int i = 0; i < initTileMap.GetLength(0); i++) {
            for (int j = 0; j < initTileMap.GetLength(1); j++) {
                var gameObj = Instantiate(tile, topLeft, Quaternion.identity);
                gameObj.name = string.Format("{0},{1}", i, j);
                var tileObj = gameObj.GetComponent<Tile>();
                tileObj.x = i;
                tileObj.y = j;
                initTileMap[i,j] = gameObj;
                topLeft.x += tileWidth / 2;
            }
            topLeft.x = origin.x - (gridSize.x / 4);
            topLeft.y -= tileWidth / 2;
        }
        tilemap.Start(this, initTileMap);
    }

    GridEntity PutEntity (int x, int y, GameObject prefab) {
        var target = tilemap.grid[x,y].GetComponent<Tile>();
        var entity = Instantiate(prefab, new Vector2(0,0), Quaternion.identity).GetComponent<GridEntity>();
        entity.GetComponent<SpriteRenderer>().sortingOrder = 1;
        target.TryOccupy(entity);
        return entity;
    }

    // TODO SIDE: "wait utility"
    private static class Coroutines {
        public static string EndTurn = "EndTurn";
        public static string ExecuteAIStep = "ExecuteAIStep";
        public static string WaitAMoment = "WaitAMoment";
    }

    // TODO SIDE: should be extracted as a single general utility function
    // IEnumerator WaitAMoment(float waitTime, string name) {...}
    IEnumerator EndTurn () {
        combat.EndTurn();
        waiting = true;
        Debug.Log("Waiting for turn gap...");
        yield return new WaitForSeconds(turnGapTime);
        Debug.Log("Done waiting for turn gap.");
        waiting = false;
    }

    IEnumerator ExecuteAIStep() {
        combat.TriggerAITurn();
        waiting = true;
        Debug.Log("Waiting for AI Step...");
        yield return new WaitForSeconds(aiStepTime);
        Debug.Log("Done waiting for AI Step.");
        waiting = false;
    }

    IEnumerator WaitAMoment() {
        waiting = true;
        Debug.Log("Waiting for a moment...");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Waiting for a moment...");
        waiting = false;
    }
 }
