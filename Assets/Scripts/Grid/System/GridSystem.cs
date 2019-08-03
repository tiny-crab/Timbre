using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour {

    public GameObject[,] initTileMap = new GameObject[8,8];
    public GameObject tile;
    public GameObject gridEntity;
    public GameObject gridNPC;
    public GameObject gridPlayer;
    public System.Random rnd = new System.Random();
    public Tile playerTile;
    public GridEntity player;
    public int selectRadius = 1;

    public bool waiting = false;
    // time between turns
    public float turnGapTime = 0.5f;
    // time between AI steps
    public float aiStepTime = 0.25f;
    // just wait whenever you feel like it
    public float waitTime = 0.1f;

    public CombatComponent combat = new CombatComponent();
    public TilemapComponent tilemap = new TilemapComponent();

    void Start () {
        CreateTilemapComponent();
        var npc = PutNPC(1,1);
        var player = PutPlayer(5,5);
        var enemyFaction = new Faction("Enemy", false, npc);
        var playerFaction = new Faction("Player", true, player);
        combat.Start(this, enemyFaction, playerFaction);
    }

    void Update () {
        if (!waiting) {
            if (combat.currentFaction.isPlayerFaction) {
                if (Input.GetMouseButtonDown(0)) {

                    // branch here:
                    // if mouse hit a game element, pass it to Combat System
                    // if mouse hit a UI element, pass it to a (unimplemented) UI System

                    Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
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
                if(Input.GetKey(KeyCode.E)) {
                    if (combat.selectedEntity != null && tilemap.skillRange.tiles.Count == 0) {
                        Debug.Log("skill activated");
                        tilemap.ActivateSkill(combat.selectedEntity);
                        StartCoroutine(Coroutines.WaitAMoment);
                    } else {
                        Debug.Log("skill deactivated");
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

    GridEntity PutNPC (int x, int y) {
        var target = tilemap.grid[x,y].GetComponent<Tile>();
        var npc = Instantiate(gridNPC, new Vector2(0,0), Quaternion.identity).GetComponent<GridEntity>();
        npc.GetComponent<SpriteRenderer>().sortingOrder = 1;
        target.TryOccupy(npc);
        return npc;
    }

    // this should also be generalized
    GridEntity PutPlayer (int x, int y) {
        var target = tilemap.grid[x,y].GetComponent<Tile>();
        player = Instantiate(gridPlayer, new Vector2(0,0), Quaternion.identity).GetComponent<GridEntity>();
        player.GetComponent<SpriteRenderer>().sortingOrder = 1;
        // ew
        if (target.TryOccupy(player)) {
            playerTile = target;
        }
        return player;
    }

    private static class Coroutines {
        public static string EndTurn = "EndTurn";
        public static string ExecuteAIStep = "ExecuteAIStep";
        public static string WaitAMoment = "WaitAMoment";
    }

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
