using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridUtils {

    public static List<Tile> GenerateTileCircle(GameObject[,] grid, int radius, Tile sourceTile) {
        List<Tile> tiles = new List<Tile>() { sourceTile };
        for (int depth = 0; depth < radius; depth++) {
            var temp = new List<Tile>();
            tiles.ForEach(tile => temp.AddRange(GetAdjacentTiles(grid, tile)));
            tiles.AddRange(temp);
        }
        tiles = tiles.Distinct().ToList();
        tiles.Remove(sourceTile);
        return tiles;
    }

    public static List<Tile> GenerateTileRing(GameObject[,] grid, int radius, Tile sourceTile) {
        return GenerateTileCircle(grid, radius, sourceTile)
        .Where(tile =>
            Mathf.Abs(Mathf.Abs(sourceTile.x) - Mathf.Abs(tile.x)) +
            Mathf.Abs(Mathf.Abs(sourceTile.y) - Mathf.Abs(tile.y))
            == radius)
        .ToList();
    }

    static List<Tile> GetAdjacentTiles (GameObject[,] grid, Tile sourceTile) {
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