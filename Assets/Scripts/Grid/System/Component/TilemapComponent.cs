using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapComponent {

    private GridSystem parent;

    public GameObject[,] grid;

    public void Start (GridSystem gridSystem, GameObject[,] initTileMap) {
        parent = gridSystem;
        grid = initTileMap;
    }

    public static void ResetTileSelection (List<Tile> tiles) {
        tiles.ForEach(t => t.RemoveHighlights());
    }

    public static void ClearAllHighlightsFromGrid (GameObject[,] grid) {
        new List<string> {
            Tile.HighlightTypes.Attack,
            Tile.HighlightTypes.Move,
            Tile.HighlightTypes.Skill,
            Tile.HighlightTypes.SkillSelect,
            Tile.HighlightTypes.Teleport,
            Tile.HighlightTypes.Test
        }.ForEach(type => ClearHighlightFromGrid(grid, type));
    }

    public static void ClearHighlightFromGrid (GameObject[,] grid, string highlightType) {
        GridUtils.FlattenGridTiles(grid).ForEach(tile => tile.currentHighlights.Remove(highlightType));
    }

    public static void RefreshGridHighlights(GameObject[,] grid, List<Tile> toHighlight, string highlightType) {
        ClearHighlightFromGrid(grid, highlightType);
        toHighlight.ForEach(x => x.HighlightAs(highlightType));
    }

    public static List<Tile> GenerateAttackRange(GameObject[,] grid, GridEntity entity) {
        var tiles = new List<Tile>();
        if (entity.currentAttacks > 0) {
            tiles = GridUtils
                .GenerateTileCircle(grid, entity.range, entity.tile)
                .Where(tile => tile.occupier != null && tile.occupier.isHostile && !tile.occupier.outOfHP)
                .ToList();
        }
        return tiles;
    }

    public static List<Tile> GenerateMoveRange(GameObject[,] grid, GridEntity entity) {
        var tiles = GridUtils
            .GenerateTileCircle(grid, entity.currentMoves, entity.tile, true)
            .Where(tile => tile.occupier == null)
            .ToList();
        return tiles;
    }

    public bool TeleportEntity(Tile origin, Tile dest) {
        var distance = GridUtils.GetPathBetweenTiles(grid, origin, dest).Count;
        if (dest.TryOccupy(origin.occupier)) {
            origin.occupier = null;
            return true;
        } else {
            Debug.Log(string.Format("Failed to teleport entity {0} to {1}", origin.occupier, dest.name));
            return false;
        }
    }

    public void MoveEntity (Tile origin, Tile dest) {
        if (TeleportEntity(origin, dest)) {
            var distance = GridUtils.GetPathBetweenTiles(grid, origin, dest).Count;
            dest.occupier.Move(distance);
        }
    }

    public Tile ClosestTile (Vector2 location) {
        return GridUtils.FlattenGridTiles(grid, true).OrderBy(tile => Vector2.Distance(location, tile.transform.position)).First();
    }
}
