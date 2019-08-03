using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapComponent {

    private GridSystem parent;

    public GameObject[,] grid;
    public GameObject tile;

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
        if (tile.occupier != null) {
            GenerateAttackRange(tile.occupier);
            GenerateMoveRange(tile.occupier);
        }
        if (skillRange.Contains(tile) && skillSelected.tiles.Count < 2) {
            if (tile.currentHighlights.Contains(Tile.HighlightTypes.SkillSelect)) {
                tile.RemoveHighlight(Tile.HighlightTypes.SkillSelect);
                skillSelected.tiles.Remove(tile);
            } else {
                tile.HighlightAs(Tile.HighlightTypes.SkillSelect);
                skillSelected.tiles.Add(tile);
            }

            if (skillSelected.tiles.Count == 2) {
                skillSelected.tiles.ForEach(t =>
                    t.hazards.Add(new Caltrops())
                );
                skillSelected.Clear(grid);
                DeactivateSkill(parent.combat.selectedEntity);
            }
        }
    }

    public void GenerateAttackRange(GridEntity entity) {
        if (entity.currentAttacks > 0) {
                attackRange.tiles = GenerateTileCircle(entity.currentMoves + entity.range, entity.tile);
                attackRange.Highlight();
        }
    }

    public void GenerateMoveRange(GridEntity entity) {
        moveRange.tiles = GenerateTileCircle(entity.currentMoves, entity.tile);
        moveRange.Highlight();
    }

    public void ActivateSkill(GridEntity activeEntity) {
        skillRange.tiles = GenerateTileCircle(3, activeEntity.tile);
        ResetTileSelection(moveRange, attackRange);
        skillRange.Highlight();
    }

    public void DeactivateSkill(GridEntity activeEntity) {
        skillRange.Clear(grid);
        GenerateAttackRange(activeEntity);
        GenerateMoveRange(activeEntity);
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

    List<Tile> GenerateTileRing(int radius, Tile sourceTile) {
        return GenerateTileCircle(radius, sourceTile)
        .Where(tile =>
            Mathf.Abs(Mathf.Abs(sourceTile.x) - Mathf.Abs(tile.x)) +
            Mathf.Abs(Mathf.Abs(sourceTile.y) - Mathf.Abs(tile.y))
            == radius)
        .ToList();
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
