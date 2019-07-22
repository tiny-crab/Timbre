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
            }
            else {
                // what is the tile they are clicking on? do they want to move? do they want to attack?
                if(targetTile.occupier != null) {
                    // clicking on another entity
                    // if entity is enemy: Attack
                    if (targetTile.occupier.isHostile && parent.tilemap.attackRangeTiles.Contains(targetTile)) {
                        // it might be nice to write this as mouseTile.occupier.Attack(damage), 
                        // but it's worthwhile to wait until later since this might not be a good idea
                        AttackEntity(targetTile.occupier, selectedEntity.attackDamage);
                    }
                    // if entity is ally: interact (to be implemented later)
                } 
                else if (parent.tilemap.moveRangeTiles.Contains(targetTile)) {
                    // if space is empty: move there (if possible)
                    parent.tilemap.MoveEntity(selectedEntity.tileX, selectedEntity.tileY, targetTile.x, targetTile.y);
                }
                selectedEntity = null; 
            }
        }
    }

    public void SelectEntity(GridEntity entity) {
        // need to check if the entity is in the current / friendly faction or not
        if (currentFaction.entities.Contains(entity)) {
            selectedEntity = entity;
        }
        else {
            // show some information about the enemy (?)
        }
    }

    public void EndTurn() {
        var previousFaction = factions.Dequeue();
        currentFaction = factions.Peek();
        factions.Enqueue(previousFaction);
    }

	void AttackEntity (GridEntity victim, int damage) {
		victim.ChangeHealth(damage * -1);
		if (victim.dead) {
			parent.tilemap.grid[victim.tileX, victim.tileY].GetComponent<Tile>().occupier = null;
		}
	}

    public void TriggerAITurn() {
        currentFaction.entities.ForEach(entity => {
            parent.tilemap.MoveEntity(
                entity.tileX, entity.tileY,
                entity.tileX + 1, entity.tileY + 1
            );
        });
    }
}