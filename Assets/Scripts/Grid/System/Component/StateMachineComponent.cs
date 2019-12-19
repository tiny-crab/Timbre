using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachineComponent {

    private GridSystem parent;

    // migrate to StateData
    public List<IGrouping<GridEntity, Behavior>> aiSteps;

    public void Start(GridSystem gridSystem) {
        parent = gridSystem;
    }

    // change this to return State
    public State SelectTile(State currentState, Tile targetTile) {

        Action action = null;
        var nextState = currentState;

        // I wonder if I can somehow make this a bit more logical
        // instead of stepping through the conditions and returning an Action/Transition,
        // can I define the conditions for an Action/Transition and return whatever Transition matches the current conditions?
        if (!(currentState is EnemyTurnState)) {
            if (currentState is NoSelectionState) {
                if (targetTile.occupier != null) {
                    action = new SelectEntity(targetTile.occupier);
                } else {
                    action = new Unselect();
                }
            }
            if (currentState is AllySelectedState) {
                var stateData = (AllySelectedState) currentState;
                // what is the tile they are clicking on? do they want to move? do they want to attack?
                if (targetTile.occupier != null) {
                    var target = targetTile.occupier;
                    // clicking on another entity
                    // if entity is enemy: Attack
                    if (target.isHostile && parent.tilemap.attackRange.Contains(targetTile)) {
                        action = new Attack(stateData.source, target);
                    }
                    // if entity is ally: interact (to be implemented later)
                }
                else if (parent.tilemap.moveRange.Contains(targetTile)) {
                    // if space is empty: move there (if possible)
                    // parent.tilemap.MoveEntity(selectedEntity.tile, targetTile);
                    action = new Move(stateData.source, targetTile);
                }
                else {
                    action = new Unselect();
                }
            }
            if (currentState is SelectSkillActivatedState) {
                var stateData = (SelectSkillActivatedState) currentState;
                var tilesInRange = stateData.validTiles.Union(stateData.selectedTiles);
                if (tilesInRange.Contains(targetTile)) {
                    action = new SelectSkillTile(stateData.source, targetTile);
                }
            }
            if (currentState is TeleportActivatedState) {
                var stateData = (TeleportActivatedState) currentState;
                if (stateData.validTiles.Contains(targetTile)) {
                    action = new SelectTeleportTile(stateData.source, targetTile);
                }
            }
        }

        if (action != null) {
            // remove all instances of parent here
            nextState = action.Transition(currentState, parent.tilemap, parent.dialog);
        }

        return nextState;
    }

    public State ActivateSkill(State currentState, int index) {

        Action action = null;
        var nextState = currentState;

        if (currentState is AllySelectedState) {
            var stateData = (AllySelectedState) currentState;
            var selectedEntity = stateData.source;
            if (index >= selectedEntity.skills.Count) { return currentState; }
            var triggeredSkill = selectedEntity.skills[index];

            if (triggeredSkill == stateData.lastActivatedSkill) {
                action = new DeactivateSkill(selectedEntity, triggeredSkill);
            } else {
                action = new ActivateSkill(selectedEntity, triggeredSkill);
                stateData.lastActivatedSkill = triggeredSkill;
            }
        }
        else if (currentState is SelectSkillActivatedState) {
            var stateData = (SelectSkillActivatedState) currentState;
            var selectedEntity = stateData.source;
            if (index >= selectedEntity.skills.Count) { return currentState; }
            var triggeredSkill = selectedEntity.skills[index];
            action = new DeactivateSkill(selectedEntity, triggeredSkill);
        }

        if (action != null) {
            // remove all instances of parent here
            nextState = action.Transition(currentState, parent.tilemap, parent.dialog);
        }

        return nextState;
    }

    public State ActivateTeleport(State currentState) {
        Action action = null;
        var nextState = currentState;

        if (currentState is AllySelectedState) {
            if (currentState.source.currentTeleports >= 0) {
                action = new ActivateTeleport(currentState.source);
            }
            else {
                // dialog.PostToDialog("Tried to teleport but " + stateMachine.selectedEntity.entityName + " has already teleported this encounter", dialogNoise, false);
            }
        }
        else if (currentState is TeleportActivatedState) {
            action = new DeactivateTeleport(currentState.source);
        }

        if (action != null) {
            // remove all instances of parent here
            nextState = action.Transition(currentState, parent.tilemap, parent.dialog);
        }

        return nextState;
    }

    public State EndTurn(State currentState, Faction currentFaction) {
        var nextState = currentState;

        currentFaction.RefreshTurnResources();

        if (currentFaction.isHostileFaction) {
            nextState = new EnemyTurnState(currentFaction.entities);
        }
        else {
            nextState = new NoSelectionState();
        }

        return nextState;
    }

    public State ExecuteAITurn(State currentState) {
        Action action = null;
        var nextState = currentState;

        if (currentState is EnemyTurnState) {
            var stateData = (EnemyTurnState) currentState;
            action = new ExecuteAIStep();
        }

        if (action != null) {
            // remove all instances of parent here
            nextState = action.Transition(currentState, parent.tilemap, parent.dialog);
        }

        return nextState;
    }

    // this needs to be pulled out
    public List<IGrouping<GridEntity, Behavior>> DetermineAITurns() {
        var aiActionsByTarget = parent.currentFaction.entities.Where(entity => !entity.outOfHP).ToList().Select(aiEntity => {
            return aiEntity.behaviors
                .OrderBy(behavior => behavior.FindBestAction(parent.tilemap.grid))
                .First();
        })
        .GroupBy(behavior => behavior.bestTarget)
        .ToList();

        return aiActionsByTarget;
    }
}

