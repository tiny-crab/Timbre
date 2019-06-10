using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridStage : MonoBehaviour {

	public GameObject[,] grid = new GameObject[8,8];
	public GameObject tile;
	public System.Random rnd = new System.Random();
	public GameObject lastClickedTile = null;
	public int selectRadius = 1;

	// Use this for initialization
	void Start () {
		Vector3 origin = new Vector3(0, 0, 0);
		// assuming tiles are squares
		float tileWidth = tile.GetComponent<SpriteRenderer>().size.x;
		Vector2 gridSize = new Vector2(tileWidth * grid.GetLength(0), tileWidth * grid.GetLength(1));

		Vector3 topLeft = new Vector3(origin.x - (gridSize.x / 4), origin.y + (gridSize.y / 4), origin.z);

		for (int i = 0; i < grid.GetLength(0); i++) {
			for (int j = 0; j < grid.GetLength(1); j++) {
				var gameObj = Instantiate(tile, topLeft, Quaternion.identity);
				gameObj.name = string.Format("{0},{1}", i, j);
				var tileObj = gameObj.GetComponent<Tile>();
				tileObj.gridX = i;
				tileObj.gridY = j;
				grid[i,j] = gameObj;
				topLeft.x += tileWidth / 2;
			}
			topLeft.x = origin.x - (gridSize.x / 4);
			topLeft.y -= tileWidth / 2;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			for (int i = 0; i < grid.GetLength(0); i++) {
				for (int j = 0; j < grid.GetLength(1); j++) {
					grid[i,j].GetComponent<Tile>().selected = false;
				}
			}
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
			Tile mouseTile = hitInfo.collider.gameObject.GetComponent<Tile>();
			GenerateTileCircle(selectRadius, mouseTile).ForEach(t => t.selected = true);
		}
	}

	List<Tile> GenerateTileCircle(int radius, Tile sourceTile) {
		List<Tile> tiles = new List<Tile>() { sourceTile };
		for (int depth = 0; depth < radius; depth++) {
			// TODO: I think these three lines could be simplified... 
			// just don't want to modify the list as it is iterating through
			var temp = new List<Tile>();
			tiles.ForEach(tile => temp.AddRange(GetAdjacentTiles(tile)));
			tiles.AddRange(temp);
		}
		return tiles.Distinct().ToList();
	}

	List<Tile> GetAdjacentTiles (Tile sourceTile) {
		var x = sourceTile.gridX;
		var y = sourceTile.gridY;
		var adjacentGameObjects = new List<GameObject>();
		if (x > 0) { adjacentGameObjects.Add(grid[x-1, y]); };
		if (x < grid.GetLength(0) - 1) { adjacentGameObjects.Add(grid[x+1, y]); };
		if (y > 0) { adjacentGameObjects.Add(grid[x, y-1]); };
		if (y < grid.GetLength(1) - 1) { adjacentGameObjects.Add(grid[x, y+1]); };
		return adjacentGameObjects.Select(o => o.GetComponent<Tile>()).ToList();
		
	}
 }
