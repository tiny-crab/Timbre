using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapComponent {

    private GridSystem parent;

    public GameObject[,] grid;
    public GameObject tile;

    public List<Tile> moveRangeTiles = new List<Tile>();
	public List<Tile> attackRangeTiles = new List<Tile>();
    
    public void Start (GridSystem gridSystem, GameObject[,] initTileMap) {
        parent = gridSystem;
        grid = initTileMap;
    }

    public void ResetTileSelection () {
        for (int i = 0; i < grid.GetLength(0); i++) {
			for (int j = 0; j < grid.GetLength(1); j++) {
				grid[i,j].GetComponent<Tile>().selected = false;
			}
		}
        this.attackRangeTiles.Clear();
        this.moveRangeTiles.Clear();
    }

    public void SelectTile(Tile tile) {
        if (tile != null && tile.occupier != null && attackRangeTiles.Count == 0) {
			moveRangeTiles = GenerateTileCircle(tile.occupier.currentMoves, tile);
			moveRangeTiles.ForEach(t => t.selected = true);
			attackRangeTiles = GenerateTileCircle(tile.occupier.range, tile);
		}
    }

    public void MoveEntity (int x0, int y0, int xDest, int yDest) {
		var origin = grid[x0, y0].GetComponent<Tile>();
		var distance = Mathf.Abs(x0-xDest) + Mathf.Abs(y0-yDest);
        if(xDest < grid.GetLength(0) && yDest < grid.GetLength(1)) {
            var dest = grid[xDest, yDest].GetComponent<Tile>();
            if (dest.TryOccupy(origin.occupier)) {
                dest.occupier = origin.occupier;
                origin.occupier = null;
				dest.occupier.Move(distance);
            }
        }
	}

    List<Tile> GenerateTileCircle(int radius, Tile sourceTile) {
		List<Tile> tiles = new List<Tile>() { sourceTile };
		for (int depth = 0; depth < radius; depth++) {
			var temp = new List<Tile>();
			tiles.ForEach(tile => temp.AddRange(GetAdjacentTiles(tile)));
			tiles.AddRange(temp);
		}
        tiles.Remove(sourceTile);
		return tiles.Distinct().ToList();
	}

	List<Tile> GetAdjacentTiles (Tile sourceTile) {
		var x = sourceTile.x;
		var y = sourceTile.y;
		var adjacentGameObjects = new List<GameObject>();
		if (x > 0) { adjacentGameObjects.Add(grid[x-1, y]); };
		if (x < grid.GetLength(0) - 1) { adjacentGameObjects.Add(grid[x+1, y]); };
		if (y > 0) { adjacentGameObjects.Add(grid[x, y-1]); };
		if (y < grid.GetLength(1) - 1) { adjacentGameObjects.Add(grid[x, y+1]); };
		return adjacentGameObjects.Select(o => o.GetComponent<Tile>()).ToList();
	}
}