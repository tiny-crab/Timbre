using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapComponent {

    private GridSystem parent;

    public GameObject[,] grid;
    public GameObject tile;

    public SelectTilesSkill activatedSkill;
    public bool selectTilesSkillCompleted = true;

    public bool teleportCompleted = true;

    public class SelectedTiles {
        public List<Tile> tiles;
        public string name;

        public SelectedTiles(string name) {
            tiles = new List<Tile>();
            this.name = name;
        }

        public void Highlight() {
            tiles.ForEach(t => t.HighlightAs(name));
        }

        // this is... a little weird but it fixes a lingering highlight bug with tiles
        public void Clear(GameObject[,] grid) {
            for (var i = 0; i < grid.GetLength(0); i++) {
                for (var j = 0; j < grid.GetLength(1); j++) {
                    grid[i,j].GetComponent<Tile>().RemoveHighlight(name);
                }
            }
            tiles.Clear();
        }

        public bool Contains(Tile element) { return tiles.Contains(element); }
    }

    public SelectedTiles testTiles = new SelectedTiles(Tile.HighlightTypes.Test);

    public void Start (GridSystem gridSystem, GameObject[,] initTileMap) {
        parent = gridSystem;
        grid = initTileMap;
    }

    public void ResetTileSelection (params SelectedTiles[] targets) {
        if (targets.Count() == 0) {
            targets = new SelectedTiles[] {
                testTiles
            };
        }
        targets.ToList().ForEach( selectedTiles => {
            selectedTiles.tiles.ForEach(t => t.RemoveHighlight(selectedTiles.name));
            selectedTiles.Clear(grid);
        });
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

    // public void SelectTile(Tile tile) {
    //     // if (tile.occupier != null && !tile.occupier.outOfHP) {
    //     //     GenerateAttackRange(tile.occupier);
    //     //     GenerateMoveRange(tile.occupier);
    //     // }
    //     if (skillRange.Contains(tile) && skillSelected.tiles.Count < activatedSkill.targets) {
    //         if (tile.currentHighlights.Contains(Tile.HighlightTypes.SkillSelect)) {
    //             tile.RemoveHighlight(Tile.HighlightTypes.SkillSelect);
    //             skillSelected.tiles.Remove(tile);
    //         } else {
    //             tile.HighlightAs(Tile.HighlightTypes.SkillSelect);
    //             skillSelected.tiles.Add(tile);
    //         }

    //         // select tiles skill completed!
    //         if (skillSelected.tiles.Count == activatedSkill.targets) {
    //             skillSelected.tiles.ForEach(x => activatedSkill.ResolveEffect(parent.stateMachine.selectedEntity, x));
    //             skillSelected.Clear(grid);
    //             selectTilesSkillCompleted = true;
    //         }
    //     }
    //     if (teleportRange.Contains(tile)) {
    //         teleportRange.Clear(grid);
    //         TeleportEntity(parent.stateMachine.selectedEntity.tile, tile);
    //         teleportCompleted = true;
    //     }
    // }

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

    public static void RefreshGridHighlights(GameObject[,] grid, List<Tile> toHighlight, string highlightType) {
        ClearHighlightFromGrid(grid, highlightType);
        toHighlight.ForEach(x => x.HighlightAs(highlightType));
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
