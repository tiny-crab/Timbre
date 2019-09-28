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

    public SelectedTiles skillSelected = new SelectedTiles(Tile.HighlightTypes.SkillSelect);

    public void Start (GridSystem gridSystem, GameObject[,] initTileMap) {
        parent = gridSystem;
        grid = initTileMap;
    }

    public void ResetTileSelection (params SelectedTiles[] targets) {
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
        moveRange.tiles = GridUtils.GenerateTileCircle(grid, entity.currentMoves, entity.tile)
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

    public Tile ClosestTile (Vector2 location) {
        return GridUtils.FlattenGridTiles(grid, true).OrderBy(tile => Vector2.Distance(location, tile.transform.position)).First();
    }
}
