using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Reaction {
    public int turnDuration;
    public abstract bool ReactsTo(GridEntity trigger);
}

public abstract class AttackReaction : Reaction {
    // return if default attack resolution should still occur
    public abstract bool ResolveAttack(GridEntity attacker, GridEntity target);
}

public class DefendSelfReaction : AttackReaction {
    public GridEntity parent;
    public List<Tile> defendAgainstMeleeTiles;

    public DefendSelfReaction(List<Tile> targets, GridEntity parent) {
        turnDuration = 1;
        defendAgainstMeleeTiles = targets;
        this.parent = parent;
    }

    public override bool ReactsTo(GridEntity trigger) {
        return defendAgainstMeleeTiles.Contains(trigger.tile) || GridUtils.GetDistanceBetweenTiles(trigger.tile, parent.tile) > 1;
    }

    public override bool ResolveAttack(GridEntity attacker, GridEntity target) {
        // target takes no damage
        Debug.Log(String.Format("<color=blue>{0}</color> attacked <color=red>{1}</color>, but <color=red>{1}</color> defended themselves.",
            attacker.entityName,
            target.entityName
        ));
        return false;
    }
}

public class RetaliateReaction : AttackReaction {
    public GridEntity parent;

    public RetaliateReaction(GridEntity parent) {
        turnDuration = 1;
        this.parent = parent;
    }

    // if entity is hit by melee attack, double damage for next player turn
    public override bool ReactsTo(GridEntity trigger) {
        return GridUtils.GetDistanceBetweenTiles(trigger.tile, parent.tile) == 1;
    }

    public override bool ResolveAttack(GridEntity attacker, GridEntity target) {
        var originalMult = target.damageMult;
        target.damageMult *= 2;

        bool RetaliateOverride() {
            target.damageMult = originalMult;
            return true;
        }

        target.overrides.Add(new GridEntity.Override("Retaliate", 1, RetaliateOverride));
        return true;
    }
}