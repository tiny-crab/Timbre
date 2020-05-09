using System.Collections.Generic;
using System.Linq;

public abstract class Action {
    public abstract State Transition(State currentState, TilemapComponent tilemap, Dialog dialog);
    // This is the "Model"
    protected abstract void Execute(State currentState, TilemapComponent tilemap);
    // This is the "View"
    protected abstract void DisplayInGrid(State currentState, TilemapComponent tilemap);
    protected abstract void DisplayInDialog(State currentState, Dialog dialog);
}

public class KeepState : Action {
    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        return currentState;
    }
    protected override void Execute(State currentState, TilemapComponent tilemap) {}
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {}
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class Unselect : Action {
    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        Execute(currentState, tilemap);
        DisplayInGrid(currentState, tilemap);
        DisplayInDialog(currentState,dialog);
        return new NoSelectionState();
    }
    protected override void Execute(State currentState, TilemapComponent tilemap) {

    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class SelectAlly : Action {
    public GridEntity source;

    public SelectAlly(GridEntity selected) {
        source = selected;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new AllySelectedState(source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;
        stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
        stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;
        source.tile.HighlightAs(Tile.HighlightTypes.SelectedEntity);
        stateData.attackRange.ForEach(x => x.HighlightAs(Tile.HighlightTypes.Attack));
        stateData.moveRange.ForEach(x => x.HighlightAs(Tile.HighlightTypes.Move));
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {
        dialog.PostToDialog("Selected " + source.entityName, null, false);
    }
}

public class SelectEnemy : Action {
    public GridEntity source;

    public SelectEnemy(GridEntity selected) {
        source = selected;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new EnemySelectedState(source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        var stateData = (EnemySelectedState) currentState;
        stateData.tileScoreMap = source.behaviors.First().ScoreGrid(tilemap.grid);
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (EnemySelectedState) currentState;
        var normalizedData = Utils.NormalizeDict(stateData.tileScoreMap);

        source.tile.HighlightAs(Tile.HighlightTypes.SelectedEntity);
        normalizedData.Keys.ToList().ForEach(x => x.tile.HighlightAs(Tile.HighlightTypes.Test, (float) normalizedData[x]));
        normalizedData.Keys.OrderBy(x => normalizedData[x]).First().tile.HighlightAs(Tile.HighlightTypes.Move);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {
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

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = currentState is AllySelectedState ? currentState : new AllySelectedState(source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        tilemap.MoveEntity(source.tile, destination);
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;

        TilemapComponent.ClearHighlightFromGrid(tilemap.grid, Tile.HighlightTypes.SelectedEntity);
        source.tile.HighlightAs(Tile.HighlightTypes.SelectedEntity);

        stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
        stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class Attack : Action {
    public GridEntity source;
    public GridEntity target;

    public Attack(GridEntity attacker, GridEntity target) {
        source = attacker;
        this.target = target;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new AllySelectedState(source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        if (source.currentAttackSkill != null) {
            source.currentAttackSkill.BeforeAttack(source, target);
            source.MakeAttack(target);
            source.currentAttackSkill.AfterAttack(source, target);
        }
        else { source.MakeAttack(target); }
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;

        stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
        stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class ActivateSkill : Action {
    public GridEntity source;
    public Skill skillToActivate;
    public List<Tile> skillRange;

    public ActivateSkill(GridEntity selectedEntity, Skill skillToActivate) {
        source = selectedEntity;
        this.skillToActivate = skillToActivate;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        Execute(currentState, tilemap);
        DisplayInGrid(currentState, tilemap);
        DisplayInDialog(currentState, dialog);
        if (skillToActivate is SelectTilesSkill) {
            return new SelectSkillActivatedState(source, (SelectTilesSkill) skillToActivate, skillRange);
        } else {
            return currentState;
        }
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        if (skillToActivate is AttackSkill) {
            source.currentAttackSkill = (AttackSkill) skillToActivate;
            //dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
        else if (skillToActivate is SelectTilesSkill) {
            skillRange = ((SelectTilesSkill) skillToActivate).GetValidTiles(tilemap.grid, source.tile);//tilemap.ActivateSelectTilesSkill(source, (SelectTilesSkill) skillToActivate);
            if (skillRange.Count() != 0) {
                // dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
            } else {
                // dialog.PostToDialog("Tried to activate " + skillToActivate.GetType().Name + " but there were no valid tiles", dialogNoise, false);
                // tilemap.DeactivateSelectTilesSkill(source);
                var stateData = (AllySelectedState) currentState;

                stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
                stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

                TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
                TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
            }
        }
        else if (skillToActivate is BuffSkill) {
            ((BuffSkill) skillToActivate).ResolveEffect(source);
            // dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        if (skillToActivate is SelectTilesSkill) {
            TilemapComponent.ClearHighlightFromGrid(tilemap.grid, Tile.HighlightTypes.Attack);
            TilemapComponent.ClearHighlightFromGrid(tilemap.grid, Tile.HighlightTypes.Move);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, skillRange, Tile.HighlightTypes.Skill);
        }

    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {
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

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new AllySelectedState(source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        if (skillToDeactivate is AttackSkill) {
            source.currentAttackSkill = null;
            //dialog.PostToDialog("Activated " + skillToActivate.GetType().Name, dialogNoise, false);
        }
        else if (skillToDeactivate is SelectTilesSkill) {
            var stateData = (AllySelectedState) currentState;

            TilemapComponent.ClearHighlightFromGrid(tilemap.grid, Tile.HighlightTypes.Skill);
            stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
            stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
        }
        else if (skillToDeactivate is BuffSkill) {
            // do nothing for now
        }
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        if (skillToDeactivate is SelectTilesSkill) {
            var stateData = (AllySelectedState) currentState;

            TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);
            stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
            stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
        }
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {
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

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = currentState;
        Execute(nextState, tilemap);
        if (completed) { nextState = new AllySelectedState(source); }
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
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
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        if (completed) {
            var stateData = (AllySelectedState) currentState;

            TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);
            stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
            stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
            TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
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
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class ActivateTeleport : Action {
    public GridEntity source;
    public List<Tile> validTiles;

    public ActivateTeleport(GridEntity selectedEntity) {
        source = selectedEntity;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        Execute(currentState, tilemap);
        DisplayInGrid(currentState, tilemap);
        DisplayInDialog(currentState, dialog);
        return new TeleportActivatedState(source, validTiles);
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        if (source.currentTeleports >= 0) {
            validTiles = GridUtils.FlattenGridTiles(tilemap.grid, true).Where(tile => tile.occupier == null).ToList();
        }
        else {
            // dialog.PostToDialog("Tried to teleport but " + source.entityName + " has already teleported this encounter", dialogNoise, false);
        }
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);
        TilemapComponent.RefreshGridHighlights(tilemap.grid, validTiles, Tile.HighlightTypes.Teleport);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class DeactivateTeleport : Action {
    public GridEntity source;

    public DeactivateTeleport(GridEntity selectedEntity) {
        source = selectedEntity;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new AllySelectedState(currentState.source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {

    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;

        TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);

        stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
        stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);

    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class SelectTeleportTile : Action {
    public GridEntity source;
    public Tile selectedTile;

    public SelectTeleportTile(GridEntity selectedEntity, Tile selectedTile) {
        source = selectedEntity;
        this.selectedTile = selectedTile;
    }

    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        var nextState = new AllySelectedState(currentState.source);
        Execute(nextState, tilemap);
        DisplayInGrid(nextState, tilemap);
        DisplayInDialog(nextState, dialog);
        return nextState;
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        currentState.source.UseTeleport();
        tilemap.TeleportEntity(currentState.source.tile, selectedTile);
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {
        var stateData = (AllySelectedState) currentState;

        TilemapComponent.ClearAllHighlightsFromGrid(tilemap.grid);

        stateData.attackRange = TilemapComponent.GenerateAttackRange(tilemap.grid, source);
        stateData.moveRange = TilemapComponent.GenerateMoveRange(tilemap.grid, source);

        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.attackRange, Tile.HighlightTypes.Attack);
        TilemapComponent.RefreshGridHighlights(tilemap.grid, stateData.moveRange, Tile.HighlightTypes.Move);
    }
    protected override void DisplayInDialog(State currentState, Dialog dialog) {}
}

public class ExecuteAIStep : Action {
    public override State Transition(State currentState, TilemapComponent tilemap, Dialog dialog) {
        Execute(currentState, tilemap);
        DisplayInGrid(currentState, tilemap);
        DisplayInDialog(currentState, dialog);

        var stateData = (EnemyTurnState) currentState;
        if (stateData.aiSteps.Count == 0) {
            // can add new reinforcement / summoning state
            return new NoSelectionState();
        } else {
            return currentState;
        }
    }

    protected override void Execute(State currentState, TilemapComponent tilemap) {
        if (currentState is EnemyTurnState) {
            var stateData = (EnemyTurnState) currentState;
            var nextStep = stateData.aiSteps.First();
            if (nextStep != null) {
                stateData.aiSteps.RemoveAt(0);
                nextStep.ToList().ForEach(behavior => behavior.DoBestAction(tilemap, currentState));
                stateData.stepTaken = nextStep;
            }
        }
    }
    protected override void DisplayInGrid(State currentState, TilemapComponent tilemap) {}
    protected override void DisplayInDialog(State currentState, Dialog dialog) {
        var stateData = (EnemyTurnState) currentState;

        // can add some more advanced dialog decisions here
        var behavior = stateData.stepTaken.ToList().First();
        dialog.PostToDialog(behavior.entity.enemyConfig.flavorText[behavior.GetType().Name]);
    }
}