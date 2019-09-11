using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatComponent {

    private GridSystem parent;

    public Faction currentFaction;
    private Queue<Faction> factions = new Queue<Faction>();

    public GridEntity selectedEntity;

    public void Start(GridSystem gridSystem, params Faction[] factions) {
        parent = gridSystem;
        foreach (var faction in factions) { this.factions.Enqueue(faction); };
        currentFaction = this.factions.First();
    }

    public void SelectTile(Tile targetTile) {
        if (currentFaction.isPlayerFaction) {
            if(selectedEntity == null) {
                if(targetTile.occupier != null) {
                    SelectEntity(targetTile.occupier);
                }
            } else {
                // what is the tile they are clicking on? do they want to move? do they want to attack?
                if(targetTile.occupier != null) {
                    // clicking on another entity
                    // if entity is enemy: Attack
                    if (targetTile.occupier.isHostile && parent.tilemap.attackRange.Contains(targetTile)) {
                        if (selectedEntity.currentAttackSkill != null) {

                            selectedEntity.currentAttackSkill.BeforeAttack(selectedEntity, targetTile.occupier);
                            selectedEntity.MakeAttack(targetTile.occupier);
                            selectedEntity.currentAttackSkill.AfterAttack(selectedEntity, targetTile.occupier);

                        }
                        else { selectedEntity.MakeAttack(targetTile.occupier); }
                    }
                    // if entity is ally: interact (to be implemented later)
                }
                else if (parent.tilemap.moveRange.Contains(targetTile)) {
                    // if space is empty: move there (if possible)
                    parent.tilemap.MoveEntity(selectedEntity.tile.x, selectedEntity.tile.y, targetTile.x, targetTile.y);
                }
                selectedEntity = null;
            }
        }
    }

    public void SelectEntity(GridEntity entity) {
        if (currentFaction.entities.Contains(entity)) {
            selectedEntity = entity;
            parent.dialog.PostToDialog("Selected " + selectedEntity.name, null, false);
        }
        else {
            // show some information about the enemy (?)
        }
    }

    public void EndTurn() {
        var previousFaction = factions.Dequeue();
        currentFaction = factions.Peek();
        factions.Enqueue(previousFaction);
        previousFaction.RefreshTurnResources();
    }

    public void TriggerAITurn() {
        currentFaction.entities.Where(entity => !entity.outOfHP).ToList().ForEach(entity => {
            // the vocabulary currently is "relative" to the characters,
            // even though the enemies are the only ones intended with AI right now
            var playerFaction = factions.ToList().Find(faction => faction.isPlayerFaction);
            var targets = playerFaction.entities;

            var targetRanges = targets.SelectMany(target => GridUtils.GenerateTileCircle(parent.tilemap.grid, entity.range, target.tile)).ToList();
            var nextTurnRange = GridUtils.GenerateTileCircle(parent.tilemap.grid, entity.maxMoves, entity.tile);

            var nextMoveMap = new Dictionary<Tile, int>();
            nextTurnRange.ForEach(tile => {
                var score = targetRanges.Select(attackTile =>
                    Mathf.Abs(tile.x-attackTile.x) + Mathf.Abs(tile.y-attackTile.y)
                ).Sum();
                nextMoveMap[tile] = score;
            });

            var nextTile = nextMoveMap.OrderBy(element => element.Value).First().Key;

            parent.tilemap.MoveEntity(
                entity.tile.x, entity.tile.y,
                nextTile.x, nextTile.y
            );

            var tileWithTarget = GridUtils.GenerateTileCircle(parent.tilemap.grid, entity.range, entity.tile)
                                .ToList()
                                .FirstOrDefault(tile => tile.occupier != null && (tile.occupier.isAllied || tile.occupier.isFriendly));
            if (tileWithTarget != null) { entity.MakeAttack(tileWithTarget.occupier); }
        });
    }
}
