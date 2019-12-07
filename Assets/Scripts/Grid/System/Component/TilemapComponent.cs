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

    // not a fan of using Tile.HighlightTypes here... seems a little tangential
    // but it does confirm the dependency between SelectedTiles and Tiles
    public SelectedTiles moveRange = new SelectedTiles(Tile.HighlightTypes.Move);
    public SelectedTiles attackRange = new SelectedTiles(Tile.HighlightTypes.Attack);
    public SelectedTiles skillRange = new SelectedTiles(Tile.HighlightTypes.Skill);
    public SelectedTiles teleportRange = new SelectedTiles(Tile.HighlightTypes.Teleport);

    public SelectedTiles skillSelected = new SelectedTiles(Tile.HighlightTypes.SkillSelect);

    public SelectedTiles testTiles = new SelectedTiles(Tile.HighlightTypes.Test);

    public void Start (GridSystem gridSystem, GameObject[,] initTileMap) {
        parent = gridSystem;
        grid = initTileMap;
    }

    public void ResetTileSelection (params SelectedTiles[] targets) {
        if (targets.Count() == 0) {
            targets = new SelectedTiles[] {
                testTiles, attackRange, moveRange, skillRange, skillSelected
            };
        }
        targets.ToList().ForEach( selectedTiles => {
            selectedTiles.tiles.ForEach(t => t.RemoveHighlight(selectedTiles.name));
            selectedTiles.Clear(grid);
        });
    }

    public void SelectTile(Tile tile) {
        if (tile.occupier != null && !tile.occupier.outOfHP) {
            GenerateAttackRange(tile.occupier);
            GenerateMoveRange(tile.occupier);
        }
        if (skillRange.Contains(tile) && skillSelected.tiles.Count < activatedSkill.targets) {
            if (tile.currentHighlights.Contains(Tile.HighlightTypes.SkillSelect)) {
                tile.RemoveHighlight(Tile.HighlightTypes.SkillSelect);
                skillSelected.tiles.Remove(tile);
            } else {
                tile.HighlightAs(Tile.HighlightTypes.SkillSelect);
                skillSelected.tiles.Add(tile);
            }

            // select tiles skill completed!
            if (skillSelected.tiles.Count == activatedSkill.targets) {
                skillSelected.tiles.ForEach(x => activatedSkill.ResolveEffect(parent.combat.selectedEntity, x));
                skillSelected.Clear(grid);
                selectTilesSkillCompleted = true;
            }
        }
        if (teleportRange.Contains(tile)) {
            teleportRange.Clear(grid);
            TeleportEntity(parent.combat.selectedEntity.tile, tile);
            teleportCompleted = true;
        }
    }

    public void GenerateAttackRange(GridEntity entity) {
        if (entity.currentAttacks > 0) {
                attackRange.tiles = GridUtils.GenerateTileCircle(grid, entity.range, entity.tile)
                                        .Where(tile => tile.occupier != null && tile.occupier.isHostile && !tile.occupier.outOfHP)
                                        .ToList();
                attackRange.Highlight();
        }
    }

    public void GenerateMoveRange(GridEntity entity) {
        moveRange.tiles = GridUtils.GenerateTileCircle(grid, entity.currentMoves, entity.tile, true)
                            .Where(tile => tile.occupier == null)
                            .ToList();
        moveRange.Highlight();
    }

    public void ActivateSelectTilesSkill(GridEntity activeEntity, SelectTilesSkill skill) {
        skillRange.tiles = skill.GetValidTiles(grid, activeEntity.tile);
        ResetTileSelection(moveRange, attackRange);
        skillRange.Highlight();
        activatedSkill = skill;
        selectTilesSkillCompleted = false;
    }

    public void DeactivateSelectTilesSkill(GridEntity activeEntity) {
        skillRange.Clear(grid);
        GenerateAttackRange(activeEntity);
        GenerateMoveRange(activeEntity);
        activatedSkill = null;
    }

    public void ActivateTeleport(GridEntity activeEntity) {
        teleportRange.tiles = GridUtils.FlattenGridTiles(grid, true).Where(tile => tile.occupier == null).ToList();
        ResetTileSelection(moveRange, attackRange);
        teleportRange.Highlight();
        teleportCompleted = false;
    }

    public void DeactivateTeleport(GridEntity activeEntity) {
        teleportRange.Clear(grid);
        GenerateAttackRange(activeEntity);
        GenerateMoveRange(activeEntity);
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
