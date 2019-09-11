using System;
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

    public List<KeyCode> skillKeys = new List<KeyCode>() { KeyCode.A, KeyCode.S, KeyCode.D };
    public Dictionary<KeyCode, int> keyToSkillIndex = new Dictionary<KeyCode, int>() {
        {KeyCode.A, 0},
        {KeyCode.S, 1},
        {KeyCode.D, 2}
    };
    private KeyCode lastPressedKey;

    public State currentState;
    // public State previousState = State.NO_SELECTION;

    // TODO SIDE: Audio + UI should be in a different class
    public Dialog dialog;
    public AudioClip dialogNoise;

    public enum State {
        NO_SELECTION,
        ALLY_SELECTED,
        ENEMY_SELECTED,
        SKILL_ACTIVATE,
        SKILL_SELECT,
        END_TURN,
        AI_TURN
    }

    void Start () {
        CreateTilemapComponent();

        knightPrefab = Resources.Load<GameObject>("Prefabs/AllyClasses/Knight");

        // TODO UP: Should just get all entities from a parent system
        // this Start() function should handle clamping their position to the nearest tile
        var npc = PutEntity(1, 1, gridNPC);
        var npc2 = PutEntity(3, 0, gridNPC);
        var player = PutEntity(5, 5, gridPlayer);
        var knight = PutEntity(4, 3, knightPrefab);
        var enemyFaction = new Faction("Enemy", false, npc, npc2);
        var playerFaction = new Faction("Player", true, player, knight);

        dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();

        combat.Start(this, playerFaction, enemyFaction);
        currentState = State.NO_SELECTION;
    }

    // TODO: restructure this to be a bit clearer - but this is the main FSM of the grid system
    // nothing else actually contain input and user behavior checks, so this is actually not bad...
    void Update () {

        var mouseTile = GetTileUnderMouse();

        if (waiting) { return; }
        switch (currentState) {

            case State.NO_SELECTION:

                if (mouseTile == null) { break; }
                else if (Input.GetMouseButtonDown(0) && mouseTile.occupier.isAllied) {
                    combat.SelectTile(mouseTile);
                    tilemap.SelectTile(mouseTile);
                    if(combat.selectedEntity == null) {
                        tilemap.ResetTileSelection(tilemap.moveRange, tilemap.attackRange);
                    }
                    currentState = State.ALLY_SELECTED;
                }
                else if (Input.GetMouseButtonDown(0) && mouseTile.occupier.isHostile) {
                    currentState = State.ENEMY_SELECTED;
                }
                else if (Input.GetKeyDown(KeyCode.G)) {
                    combat.EndTurn();
                    StartCoroutine(WaitAMoment(turnGapTime, "Ending Player Turn"));
                    currentState = State.END_TURN;
                }
                break;


            case State.ALLY_SELECTED:

                if (mouseTile == null) { break; }
                else if (Input.GetMouseButtonDown(0)) {
                    combat.SelectTile(mouseTile);
                    tilemap.SelectTile(mouseTile);
                    if(combat.selectedEntity == null) {
                        tilemap.ResetTileSelection(tilemap.moveRange, tilemap.attackRange);
                    }
                    currentState = State.NO_SELECTION;
                }
                else if ( skillKeys.Any(key => Input.GetKeyDown(key))) {
                    if (combat.selectedEntity != null && tilemap.skillRange.tiles.Count == 0) {
                        skillKeys.ForEach(keyPressed => {
                            if (Input.GetKeyDown(keyPressed)) {
                                ActivateSkill(combat.selectedEntity, keyToSkillIndex[keyPressed]);
                                lastPressedKey = keyPressed;
                            }
                        });
                        StartCoroutine(WaitAMoment(waitTime, "Skill Activation"));

                    }
                }
                break;


            case State.ENEMY_SELECTED:

                if (mouseTile == null) { break; }
                else if (Input.GetMouseButtonDown(0) && mouseTile.occupier == null) {
                    currentState = State.NO_SELECTION;
                }
                break;


            case State.SKILL_ACTIVATE:

                if (Input.GetMouseButtonDown(0)) {
                    tilemap.SelectTile(mouseTile);
                }
                else if (Input.GetKeyDown(lastPressedKey)) {
                    DeactivateSkill(combat.selectedEntity, keyToSkillIndex[lastPressedKey]);
                    StartCoroutine(WaitAMoment(waitTime, "Skill Deactivation"));
                    lastPressedKey = KeyCode.E;
                    currentState = State.ALLY_SELECTED;
                }
                if (tilemap.skillRange.tiles.Count() == 0) {
                    StartCoroutine(WaitAMoment(waitTime, "Skill Deactivation"));
                    lastPressedKey = KeyCode.E;
                    currentState = State.ALLY_SELECTED;
                }
                // this is duplicated from the "ally selected" state in order to allow this state to loop back to itself
                else {
                    skillKeys.ForEach(keyPressed => {
                        if (Input.GetKeyDown(keyPressed)) {
                            ActivateSkill(combat.selectedEntity, keyToSkillIndex[keyPressed]);
                            lastPressedKey = keyPressed;
                        }
                    });
                }

                break;


            case State.END_TURN:

                if (combat.currentFaction.isHostileFaction) {
                    combat.TriggerAITurn();
                    currentState = State.AI_TURN;
                }
                else if (combat.currentFaction.isPlayerFaction) {
                    currentState = State.NO_SELECTION;
                }
                break;


            case State.AI_TURN:

                combat.EndTurn();
                StartCoroutine(WaitAMoment(aiStepTime, "Ending AI Turn"));
                currentState = State.END_TURN;
                break;


            default:
                currentState = State.NO_SELECTION;
                break;
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

    public static Tile GetTileUnderMouse() {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
        var collider = hitInfo.collider;
        if (collider != null) {
            return hitInfo.collider.gameObject.GetComponent<Tile>();
        } else {
            return null;
        }
    }

    GridEntity PutEntity (int x, int y, GameObject prefab) {
        var target = tilemap.grid[x,y].GetComponent<Tile>();
        var entity = Instantiate(prefab, new Vector2(0,0), Quaternion.identity).GetComponent<GridEntity>();
        entity.GetComponent<SpriteRenderer>().sortingOrder = 1;
        target.TryOccupy(entity);
        return entity;
    }

    void ActivateSkill(GridEntity selectedEntity, int index) {
        if (index >= selectedEntity.skills.Count) { return; }
        var skillToActivate = selectedEntity.skills[index];
        if (skillToActivate is AttackSkill) {
            combat.selectedEntity.currentAttackSkill = (AttackSkill) skillToActivate;
            dialog.PostToDialog("Activated " + selectedEntity.skillNames[index], dialogNoise, false);
        }
        else if (skillToActivate is SelectTilesSkill) {
            tilemap.ActivateSkill(combat.selectedEntity, (SelectTilesSkill) skillToActivate);
            if (tilemap.skillRange.tiles.Count() != 0) {
                currentState = State.SKILL_ACTIVATE;
                dialog.PostToDialog("Activated " + selectedEntity.skillNames[index], dialogNoise, false);
            } else {
                dialog.PostToDialog("Tried to activate " + selectedEntity.skillNames[index] + " but there were no valid tiles", dialogNoise, false);
                tilemap.DeactivateSkill(combat.selectedEntity);
            }
        }
    }

    void DeactivateSkill(GridEntity selectedEntity, int index) {
        if (index >= selectedEntity.skills.Count) { return; }
        var skillToDeactivate = selectedEntity.skills[index];
        if (skillToDeactivate is AttackSkill) {
            combat.selectedEntity.currentAttackSkill = null;
        }
        else if (skillToDeactivate is SelectTilesSkill) {
            tilemap.DeactivateSkill(combat.selectedEntity);
            currentState = State.ALLY_SELECTED;
        }
        dialog.PostToDialog("Deactivated " + selectedEntity.skillNames[index], dialogNoise, false);
    }

    IEnumerator WaitAMoment(float waitTime, string name) {
        waiting = true;
        Debug.Log("Waiting for... " + name);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Waiting for... " + name);
        waiting = false;
    }
 }
