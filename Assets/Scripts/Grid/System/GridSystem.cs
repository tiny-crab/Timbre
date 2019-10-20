using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour {

    public bool activated = false;

    public new Camera camera;

    public int tileMapSize = 10;
    public GameObject[,] initTileMap;
    public GameObject tile;

    // TODO UP: would be passed from a context in the overworld
    public GameObject gridEntity;
    public GameObject gridNPC;
    public GameObject gridPlayer;
    public GameObject knightPrefab;

    // this is less important once more allies are on the field
    public GridEntity player;
    public Player overworldPlayer;

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
        SELECT_SKILL_ACTIVATED,
        END_TURN,
        AI_TURN
    }

    public void ActivateGrid (Vector2 playerLocation, List<GameObject> activeParty) {
        activated = true;
        waiting = false;
        GridUtils.FlattenGridTiles(tilemap.grid)
            .Where(tile => !tile.disabled)
            .Where(tile => {
                Vector3 screenPoint = camera.WorldToViewportPoint(tile.transform.position);
                var leftRightMargin = .2f;
                var topBottomMargin = .15f;
                return
                    screenPoint.z > 0 &&
                    screenPoint.x > 0 + leftRightMargin && screenPoint.x < 1 - leftRightMargin &&
                    screenPoint.y > 0 + topBottomMargin && screenPoint.y < 1 - topBottomMargin;
            }).ToList()
            .ForEach(tile => tile.gameObject.SetActive(true));

        // snap player into Grid
        var playerPrefab = activeParty.Find(obj => obj.name == "GridPlayer");
        var closestTile = tilemap.ClosestTile(playerLocation);
        player = PutEntity(closestTile.x, closestTile.y, playerPrefab);

        // snap party into Grid
        var party = new List<GridEntity>() { player };
        activeParty.Where(obj => obj.name != "GridPlayer").ToList().ForEach(entity => {
            var adjacentTile = GridUtils.GenerateTileSquare(tilemap.grid, 1, player.tile)
                                    .Where(tile => tile.occupier == null)
                                    .First();
            party.Add(PutEntity(adjacentTile.x, adjacentTile.y, entity));
        });

        var randomTile = GridUtils.GetRandomEnabledTile(tilemap.grid);
        var npc = PutEntity(randomTile.x, randomTile.y, gridNPC);

        randomTile = GridUtils.GetRandomEnabledTile(tilemap.grid);
        var npc2 = PutEntity(randomTile.x, randomTile.y, gridNPC);
        npc2.gameObject.name = npc2.gameObject.name + "2";
        // npc2.behaviors = new List<Behavior>() { new RangedAttackV1() };

        var enemyFaction = new Faction("Enemy", false, npc, npc2);
        var playerFaction = new Faction("Player", true, party);

        combat.Start(this, playerFaction, enemyFaction);
        currentState = State.NO_SELECTION;
    }

    public void DeactivateGrid () {
        activated = false;
        waiting = true;
        GridUtils.FlattenGridTiles(tilemap.grid)
            .Where(tile => !tile.disabled).ToList()
            .ForEach(tile => tile.gameObject.SetActive(false));

        combat.factions.ToList().ForEach(faction => {
            faction.entities.ForEach(entity => entity.RemoveFromGrid());
        });
        combat.factions = new Queue<Faction>();
    }

    void Awake () {
        initTileMap = new GameObject[tileMapSize, tileMapSize];
    }

    void Start () {
        CreateTilemapComponent();
        knightPrefab = Resources.Load<GameObject>("Prefabs/AllyClasses/Knight");
        dialog = (Dialog) GameObject.Find("Dialog").GetComponent<Dialog>();
        DeactivateGrid();
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
                else if ( InputUtils.GetKeyPressed(skillKeys) != null ) {
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


            case State.SELECT_SKILL_ACTIVATED:

                if (Input.GetMouseButtonDown(0)) {
                    tilemap.SelectTile(mouseTile);
                }
                else if (Input.GetKeyDown(lastPressedKey)) {
                    // select skill is canceled
                    DeactivateSkill(combat.selectedEntity, keyToSkillIndex[lastPressedKey]);
                    StartCoroutine(WaitAMoment(waitTime, "Skill Deactivation"));
                    lastPressedKey = KeyCode.E;
                    currentState = State.ALLY_SELECTED;
                }
                if (tilemap.selectTilesSkillCompleted) {
                    // select skill is complete
                    StartCoroutine(WaitAMoment(waitTime, "Skill Deactivation"));
                    DeactivateSkill(combat.selectedEntity, keyToSkillIndex[lastPressedKey]);
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
                gameObj.transform.parent = this.transform;
                gameObj.name = String.Format("{0},{1}", i, j);

                var boxCollider = gameObj.GetComponent<BoxCollider2D>();
                Collider2D[] colliders = new Collider2D[10];
                boxCollider.OverlapCollider(new ContactFilter2D().NoFilter(), colliders);

                var tileObj = gameObj.GetComponent<Tile>();

                tileObj.x = i;
                tileObj.y = j;
                initTileMap[i,j] = gameObj;
                topLeft.x += tileWidth / 2;

                // determining if placement is in game-bounds
                tileObj.disabled = true;
                gameObj.SetActive(false);

                if (
                    colliders.Where(collider => collider != null).Any(collider => collider.tag == "EnableGrid") &&
                    !colliders.Where(collider => collider != null).Any(collider => collider.tag == "BlockGridMovement")
                ) {
                    tileObj.disabled = false;
                    gameObj.SetActive(true);
                }
            }
            topLeft.x = origin.x - (gridSize.x / 4);
            topLeft.y -= tileWidth / 2;
        }
        tilemap.Start(this, initTileMap);
    }

    public static Tile GetTileUnderMouse() {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(mouseWorldPosition, Vector2.zero).ToList();
        var objs = hits.Select(hit => hit.collider.gameObject);
        var tiles = objs.Select(obj => obj.GetComponent<Tile>()).Where(obj => obj != null).ToList();
        if (tiles.Count() != 0) { return tiles.First(); }
        else { return null; }
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
        if (skillToActivate.cost > selectedEntity.currentSP) {
            dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough SP", dialogNoise, false);
        }
        else if (selectedEntity.outOfSkillUses) {
            dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough skill uses", dialogNoise, false);
        }
        else if (skillToActivate is AttackSkill) {
            ActivateAttackSkill(selectedEntity, (AttackSkill) skillToActivate);
        }
        else if (skillToActivate is SelectTilesSkill) {
            ActivateSelectTilesSkill(selectedEntity, (SelectTilesSkill) skillToActivate);
        }
        else if (skillToActivate is BuffSkill) {
            ActivateBuffSkill(selectedEntity, (BuffSkill) skillToActivate);
        }
    }

    void DeactivateSkill(GridEntity selectedEntity, int index) {
        if (index >= selectedEntity.skills.Count) { return; }
        var skillToDeactivate = selectedEntity.skills[index];
        if (skillToDeactivate is AttackSkill) {
            DeactivateAttackSkill();
        }
        else if (skillToDeactivate is SelectTilesSkill) {
            DeactivateSelectTilesSkill();
        }
        else if (skillToDeactivate is BuffSkill) {
            DeactivateBuffSkill();
        }
        dialog.PostToDialog("Deactivated " + skillToDeactivate.GetType().Name, dialogNoise, false);
    }

    void ActivateSelectTilesSkill(GridEntity selectedEntity, SelectTilesSkill skillToActivate) {
        tilemap.ActivateSelectTilesSkill(combat.selectedEntity, skillToActivate);
        if (tilemap.skillRange.tiles.Count() != 0) {
            currentState = State.SELECT_SKILL_ACTIVATED;
            dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        } else {
            dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but there were no valid tiles", dialogNoise, false);
            tilemap.DeactivateSelectTilesSkill(combat.selectedEntity);
        }
    }

    void DeactivateSelectTilesSkill() {
        if (tilemap.selectTilesSkillCompleted) { combat.selectedEntity.UseSkill(tilemap.activatedSkill); }
        tilemap.DeactivateSelectTilesSkill(combat.selectedEntity);
        currentState = State.ALLY_SELECTED;
    }

    void ActivateAttackSkill(GridEntity selectedEntity, AttackSkill skillToActivate) {
        combat.selectedEntity.currentAttackSkill = (AttackSkill) skillToActivate;
        dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
    }

    void DeactivateAttackSkill() {
        combat.selectedEntity.currentAttackSkill = null;
    }

    void ActivateBuffSkill(GridEntity selectedEntity, BuffSkill skillToActivate) {
        skillToActivate.ResolveEffect(selectedEntity);
        dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
    }

    void DeactivateBuffSkill() {}

    IEnumerator WaitAMoment(float waitTime, string name) {
        waiting = true;
        Debug.Log("Waiting for... " + name);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Waiting for... " + name);
        waiting = false;
    }
 }
