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
	public GridEntity selectedEntity;
	public int selectRadius = 1;

	public CombatComponent combat = new CombatComponent();
	public TilemapComponent tilemap = new TilemapComponent();

	void Start () {
		CreateTilemapComponent();
		var npc = PutNPC(1,1);
		var player = PutPlayer(1,2);
		var enemyFaction = new Faction("Enemy", false, npc);
		var playerFaction = new Faction("Player", true, player);
		combat.Start(this, enemyFaction, playerFaction);
	}
	
	void Update () {
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
						tilemap.ResetTileSelection();
					}
				}
			}
			if(Input.GetKey(KeyCode.G)) {
				combat.EndTurn();
			}
		}
		if (combat.currentFaction.isHostileFaction) {
			combat.currentFaction.TriggerAITurn();
			combat.EndTurn();
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
 }
