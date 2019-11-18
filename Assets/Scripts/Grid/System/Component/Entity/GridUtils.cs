using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridUtils {

    public static int GetDistanceBetweenTiles(Tile origin, Tile dest) {
        return Mathf.Abs(Mathf.Abs(origin.x) - Mathf.Abs(dest.x)) + Mathf.Abs(Mathf.Abs(origin.y) - Mathf.Abs(dest.y));
    }

    // public static int GetWalkingDistanceBetweenTiles(GameObject[,] grid, Tile origin, Tile dest) {

    // }

    public static List<Tile> GetPathBetweenTiles(GameObject[,] grid, Tile origin, Tile dest, bool onlyActiveTiles = true) {

        int Heuristic(Tile tile) {
            return 0;
        }

        List<Tile> ReconstructPath(Dictionary<Tile, Tile> pathDict, Tile current) {
            var totalPath = new List<Tile> { current };
            while (pathDict.Keys.Contains(current)) {
                current = pathDict[current];
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
        }

        var open = new List<Tile> { origin };

        var cameFrom = new Dictionary<Tile, Tile>();
        var gScore = new Dictionary<Tile, int> {
            { origin, 0 }
        };
        var fScore = new Dictionary<Tile, int> {
            { origin, Heuristic(origin) }
        };

        var iterations = 0;

        while (open.Count() != 0) {
            var currentTile = open.OrderBy(tile => fScore[tile]).First();
            if (currentTile == dest) {
                return ReconstructPath(cameFrom, currentTile);
            }
            open.Remove(currentTile);
            var neighbors = GetAdjacentTiles(grid, currentTile).Where(tile => tile.enabled).ToList();
            if (onlyActiveTiles) {
                neighbors = neighbors.Where(tile => tile.gameObject.activeInHierarchy).ToList();
            }
            neighbors.ForEach(neighbor => {
                var tentativeGScore = gScore[currentTile] + 1;
                if (!gScore.Keys.Contains(neighbor)) {
                    gScore.Add(neighbor, int.MaxValue);
                }
                if (tentativeGScore < gScore[neighbor]) {
                    cameFrom[neighbor] = currentTile;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor);
                    if (!open.Contains(neighbor)) {
                        open.Add(neighbor);
                    }
                }
            });
            iterations++;
            if (iterations == 1000) {
                Debug.Log("Pathfinding infinitely looped");
                break;
            }
        }

        // empty path is considered failure
        return new List<Tile>();
    }

    public static List<Tile> GenerateTileCircle(GameObject[,] grid, int radius, Tile sourceTile, bool movement = false) {
        var circle = GridUtils.FlattenGridTiles(grid, true).Where(tile => GetDistanceBetweenTiles(tile, sourceTile) <= radius).ToList();
        if (movement) {
            circle = circle.Where(tile => {
                // this path length is decremented because it contains source tile
                var pathLength = GetPathBetweenTiles(grid, sourceTile, tile, true).Count() - 1;
                return pathLength > 0 && pathLength <= radius;
            }).ToList();
        }
        return circle;
    }

    // TODO optimize this when it is beginning to lag more
    // at the moment it's only used in determining adjacent tiles for party placement
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

    public static List<Tile> GetEdgesOfEnabledGrid (GameObject[,] grid) {
        var allEnabledTiles = FlattenGridTiles(grid, true).Where(tile => tile.gameObject.activeInHierarchy);
        var columnValues = allEnabledTiles.Select(tile => tile.x);
        var rowValues = allEnabledTiles.Select(tile => tile.y);

        var eastWestEdges = rowValues.SelectMany(rowId => {
            var row = allEnabledTiles.Where(tile => tile.y == rowId).OrderBy(tile => tile.x);
            return new List<Tile>() {
                { row.First() },
                { row.Last() }
            };
        });

        var northSouthEdges = columnValues.SelectMany(columnId => {
            var column = allEnabledTiles.Where(tile => tile.x == columnId).OrderBy(tile => tile.y);
            return new List<Tile>() {
                { column.First() },
                { column.Last() }
            };
        });

        return eastWestEdges.Concat(northSouthEdges).ToList();
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