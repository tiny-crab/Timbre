using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatComponent {

    private GridSystem parent;

    public Faction currentFaction;
    public Queue<Faction> factions = new Queue<Faction>();

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
                    var target = targetTile.occupier;

                    // clicking on another entity
                    // if entity is enemy: Attack
                    if (target.isHostile && parent.tilemap.attackRange.Contains(targetTile)) {
                        if (selectedEntity.currentAttackSkill != null) {

                            selectedEntity.currentAttackSkill.BeforeAttack(selectedEntity, target);
                            selectedEntity.MakeAttack(target);
                            selectedEntity.currentAttackSkill.AfterAttack(selectedEntity, target);

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
            parent.dialog.PostToDialog("Selected " + selectedEntity.entityName, null, false);
        }
        else {
            // show some information about the enemy (?)
        }
    }

    public void EndTurn() {
        var previousFaction = factions.Dequeue();
        currentFaction = factions.Peek();
        currentFaction.RefreshTurnResources();
        factions.Enqueue(previousFaction);
    }

    public void TriggerAITurn() {
        currentFaction.entities.Where(entity => !entity.outOfHP).ToList().ForEach(aiEntity => {
            aiEntity.behaviors
                .OrderBy(behavior => behavior.FindBestAction(parent.tilemap.grid))
                .First()
                .DoBestAction(this, parent.tilemap);
        });
    }
}
