using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridUtils {

    public static int GetDistanceBetweenTiles(Tile origin, Tile dest) {
        return Mathf.Abs(Mathf.Abs(origin.x) - Mathf.Abs(dest.x)) + Mathf.Abs(Mathf.Abs(origin.y) - Mathf.Abs(dest.y));
    }

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

    public static List<Tile> GenerateTileSquare(GameObject[,] grid, int radius, Tile sourceTile) {
        List<Tile> tiles = new List<Tile>() { sourceTile };
        for (int depth = 0; depth < radius; depth++) {
            var temp = new List<Tile>();
            tiles.ForEach(tile => temp.AddRange(GetAdjacentTiles(grid, tile).Concat(GetDiagonalTiles(grid, tile))));
            tiles.AddRange(temp);
        }
        tiles = tiles.Distinct().ToList();
        tiles.Remove(sourceTile);
        return tiles;
    }

    public static List<Tile> GenerateTileRing(GameObject[,] grid, int radius, Tile sourceTile) {
        return GenerateTileCircle(grid, radius, sourceTile)
            .Where(tile => GetDistanceBetweenTiles(sourceTile, tile) == radius)
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
        return adjacentGameObjects.Select(o => o.GetComponent<Tile>()).Where(tile => !tile.disabled).ToList();
    }

    static List<Tile> GetDiagonalTiles (GameObject[,] grid, Tile sourceTile) {
        var x = sourceTile.x;
        var y = sourceTile.y;
        var adjacentGameObjects = new List<GameObject>();
        if (x < grid.GetLength(0) - 1 && y < grid.GetLength(1) - 1) { adjacentGameObjects.Add(grid[x+1, y+1]); }
        if (x > 0 && y > 0) { adjacentGameObjects.Add(grid[x-1, y-1]); }
        if (x > 0 && y < grid.GetLength(1) - 1) { adjacentGameObjects.Add(grid[x-1, y+1]); }
        if (x < grid.GetLength(0) - 1 && y > 0) { adjacentGameObjects.Add(grid[x+1, y-1]); }
        return adjacentGameObjects.Select(o => o.GetComponent<Tile>()).Where(tile => !tile.disabled).ToList();
    }

    public static List<Tile> FlattenGridTiles(GameObject[,] grid, bool onlyEnabled=false) {
        var aggregate = new List<Tile>();
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                var tile = grid[i,j].GetComponent<Tile>();
                if (!onlyEnabled || !tile.disabled) { aggregate.Add(tile); }
            }
        }
        return aggregate;
    }

    public static Tile GetRandomEnabledTile(GameObject[,] grid) {
        var rnd = new System.Random();
        var randomTileOrder = GridUtils.FlattenGridTiles(grid, onlyEnabled: true)
                                .Where(tile => tile.gameObject.activeInHierarchy)
                                .OrderBy(x => rnd.Next());
        return randomTileOrder.First();
    }

}