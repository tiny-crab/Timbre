using System;
using System.Collections.Generic;
using System.Linq;

// How do Actions and States coalesce?

// It's a bit weird how Actions have source/target grid entities but also StateData has grid entities stored as well
//      One difference is that Actions only use the member variables for context within the Action and don't pass them on (I think)
//      StateData is also more static... I think

public abstract class Action {
    // would be very nice to just call Transition and get the next state -
    // the other methods in here being private
    public abstract State Transition(State currentState);
    // This is the "Model"
    public abstract void Execute(State currentState, TilemapComponent tilemap);
    // This is the "View"
    public abstract void DisplayInGrid(TilemapComponent tilemap);
    public abstract void DisplayInDialog(Dialog dialog);
}

public class KeepState : Action {
    public override State Transition(State currentState) { return currentState; }
    public override void Execute(State currentState, TilemapComponent tilemap) {}
    public override void DisplayInGrid(TilemapComponent tilemap) {}
    public override void DisplayInDialog(Dialog dialog) {}
}

public class Unselect : Action {
    public override State Transition(State currentState) {
        return new NoSelectionState();
    }
    public override void Execute(State currentState, TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {}
    public override void DisplayInDialog(Dialog dialog) {}
}

public class SelectEntity : Action {
    public GridEntity source;

    public SelectEntity(GridEntity selected) {
        source = selected;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(source);
    }
    public override void Execute(State currentState, TilemapComponent tilemap) {}
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.GenerateAttackRange(source);
        tilemap.GenerateMoveRange(source);
    }
    public override void DisplayInDialog(Dialog dialog) {
        dialog.PostToDialog("Selected " + source.entityName, null, false);
    }
}

public class Move : Action {
    public GridEntity source;
    public Tile destination;

    public Move(GridEntity selected, Tile destination) {
        source = selected;
        this.destination = destination;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(source);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        tilemap.MoveEntity(source.tile, destination);
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
        tilemap.GenerateAttackRange(source);
        tilemap.GenerateMoveRange(source);
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class Attack : Action {
    public GridEntity source;
    public GridEntity target;

    public Attack(GridEntity attacker, GridEntity target) {
        source = attacker;
        this.target = target;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(source);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        if (source.currentAttackSkill != null) {
            source.currentAttackSkill.BeforeAttack(source, target);
            source.MakeAttack(target);
            source.currentAttackSkill.AfterAttack(source, target);
        }
        else { source.MakeAttack(target); }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
        tilemap.GenerateAttackRange(source);
        tilemap.GenerateMoveRange(source);
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class ActivateSkill : Action {
    public GridEntity source;
    public Skill skillToActivate;
    public List<Tile> skillRange;

    public ActivateSkill(GridEntity selectedEntity, Skill skillToActivate) {
        source = selectedEntity;
        this.skillToActivate = skillToActivate;
    }

    public override State Transition(State currentState) {
        if (skillToActivate is SelectTilesSkill) {
            return new SelectSkillActivatedState(source, (SelectTilesSkill) skillToActivate, skillRange);
        } else {
            return currentState;
        }
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        if (skillToActivate is AttackSkill) {
            source.currentAttackSkill = (AttackSkill) skillToActivate;
            //dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
        else if (skillToActivate is SelectTilesSkill) {
            skillRange = ((SelectTilesSkill) skillToActivate).GetValidTiles(tilemap.grid, source.tile);//tilemap.ActivateSelectTilesSkill(source, (SelectTilesSkill) skillToActivate);
            if (tilemap.skillRange.tiles.Count() != 0) {
                // dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
            } else {
                // dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but there were no valid tiles", dialogNoise, false);
                tilemap.DeactivateSelectTilesSkill(source);
            }
        }
        else if (skillToActivate is BuffSkill) {
            ((BuffSkill) skillToActivate).ResolveEffect(source);
            // dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        if (skillToActivate is SelectTilesSkill) {
            tilemap.ResetTileSelection();
            skillRange.ForEach(x => x.HighlightAs(Tile.HighlightTypes.Skill));
        }
    }
    public override void DisplayInDialog(Dialog dialog) {
        if (skillToActivate.cost > source.currentSP) {
            // dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough SP", dialogNoise, false);
            dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough SP");
        }
        else if (source.outOfSkillUses) {
            // dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough skill uses", dialogNoise, false);
            dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but not enough skill uses");
        }
        else {
            dialog.PostToDialog("Activated " + skillToActivate.GetType().Name + ".");
        }
    }
}

public class DeactivateSkill : Action {
    public GridEntity source;
    public Skill skillToDeactivate;

    public DeactivateSkill(GridEntity selectedEntity, Skill skillToDeactivate) {
        source = selectedEntity;
        this.skillToDeactivate = skillToDeactivate;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(source);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        if (skillToDeactivate is AttackSkill) {
            source.currentAttackSkill = null;
            //dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
        else if (skillToDeactivate is SelectTilesSkill) {
            if (tilemap.selectTilesSkillCompleted) { source.UseSkill(skillToDeactivate); }
            tilemap.DeactivateSelectTilesSkill(source);
        }
        else if (skillToDeactivate is BuffSkill) {
            // do nothing for now
        }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        if (skillToDeactivate is SelectTilesSkill) {
            tilemap.ResetTileSelection();
            tilemap.GenerateAttackRange(source);
            tilemap.GenerateMoveRange(source);
        }
    }
    public override void DisplayInDialog(Dialog dialog) {
        dialog.PostToDialog("Deactivated " + skillToDeactivate.GetType().Name + ".");
    }
}

public class SelectSkillTile : Action {
    public GridEntity source;
    public Tile selectedTile;
    private bool completed;

    public SelectSkillTile(GridEntity selectedEntity, Tile selectedTile) {
        source = selectedEntity;
        this.selectedTile = selectedTile;
    }

    public override State Transition(State currentState) {
        if (completed) {
            return new AllySelectedState(source);
        } else {
            return currentState;
        }
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        var stateData = (SelectSkillActivatedState) currentState;
        // selecting a tile
        if (stateData.validTiles.Contains(selectedTile)) {
            stateData.selectedTiles.Add(selectedTile);
            stateData.validTiles.Remove(selectedTile);
        }
        // deselecting a tile
        else {
            stateData.validTiles.Add(selectedTile);
            stateData.selectedTiles.Remove(selectedTile);
        }

        completed = stateData.selectedTiles.Count == stateData.activeSkill.targets;

        if (completed) {
            source.UseSkill(stateData.activeSkill);
            stateData.selectedTiles.ForEach(x => stateData.activeSkill.ResolveEffect(source, x));
        }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        if (completed) {
            tilemap.ResetTileSelection();
            tilemap.GenerateMoveRange(source);
            tilemap.GenerateAttackRange(source);
        } else {
            // selecting a tile
            if (selectedTile.currentHighlights.Contains(Tile.HighlightTypes.Skill)) {
                selectedTile.currentHighlights.Remove(Tile.HighlightTypes.Skill);
                selectedTile.currentHighlights.Add(Tile.HighlightTypes.SkillSelect);
            }
            // deselecting a tile
            else {
                selectedTile.currentHighlights.Remove(Tile.HighlightTypes.SkillSelect);
                selectedTile.currentHighlights.Add(Tile.HighlightTypes.Skill);
            }
        }
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class ActivateTeleport : Action {
    public GridEntity source;
    public List<Tile> validTiles;

    public ActivateTeleport(GridEntity selectedEntity) {
        source = selectedEntity;
    }

    public override State Transition(State currentState) {
        return new TeleportActivatedState(source, validTiles);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        if (source.currentTeleports >= 0) {
            validTiles = GridUtils.FlattenGridTiles(tilemap.grid, true).Where(tile => tile.occupier == null).ToList();
        }
        else {
            // dialog.PostToDialog("Tried to teleport but " + source.entityName + " has already teleported this encounter", dialogNoise, false);
        }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
        validTiles.ForEach(x => x.HighlightAs(Tile.HighlightTypes.Teleport));
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class DeactivateTeleport : Action {
    public GridEntity source;

    public DeactivateTeleport(GridEntity selectedEntity) {
        source = selectedEntity;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(currentState.source);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {

    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
        tilemap.GenerateMoveRange(source);
        tilemap.GenerateAttackRange(source);
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class SelectTeleportTile : Action {
    public GridEntity source;
    public Tile selectedTile;

    public SelectTeleportTile(GridEntity selectedEntity, Tile selectedTile) {
        source = selectedEntity;
        this.selectedTile = selectedTile;
    }

    public override State Transition(State currentState) {
        return new AllySelectedState(currentState.source);
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        currentState.source.UseTeleport();
        tilemap.TeleportEntity(currentState.source.tile, selectedTile);
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {
        tilemap.ResetTileSelection();
        tilemap.GenerateMoveRange(source);
        tilemap.GenerateAttackRange(source);
    }
    public override void DisplayInDialog(Dialog dialog) {}
}

public class ExecuteAIStep : Action {

    public override State Transition(State currentState) {
        var stateData = (EnemyTurnState) currentState;
        if (stateData.aiSteps.Count == 0) {
            return new NoSelectionState();
        } else {
            return currentState;
        }
    }

    public override void Execute(State currentState, TilemapComponent tilemap) {
        if (currentState is EnemyTurnState) {
            var stateData = (EnemyTurnState) currentState;
            var nextStep = stateData.aiSteps.First();
            if (nextStep != null) {
                stateData.aiSteps.RemoveAt(0);
                nextStep.ToList().ForEach(behavior => behavior.DoBestAction(tilemap, currentState));
            }
        }
    }
    public override void DisplayInGrid(TilemapComponent tilemap) {}
    public override void DisplayInDialog(Dialog dialog) {}
}