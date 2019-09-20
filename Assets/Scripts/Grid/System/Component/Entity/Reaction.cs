using System;
using System.Collections.Generic;
using UnityEngine;

public interface Reaction {
    bool ReactsTo(GridEntity trigger);
}

public abstract class AttackReaction : Reaction {
    public abstract bool ReactsTo(GridEntity trigger);
    public abstract void ResolveAttack(GridEntity attacker, GridEntity target);
}

public class DefendSelfReaction : AttackReaction {
    public GridEntity parent;
    public List<Tile> defendAgainstMeleeTiles;

    public DefendSelfReaction(List<Tile> targets, GridEntity parent) {
        defendAgainstMeleeTiles = targets;
        this.parent = parent;
    }

    public override bool ReactsTo(GridEntity trigger) {
        return defendAgainstMeleeTiles.Contains(trigger.tile) || GridUtils.GetDistanceBetweenTiles(trigger.tile, parent.tile) > 1;
    }

    public override void ResolveAttack(GridEntity attacker, GridEntity target) {
        // target takes no damage
        Debug.Log(String.Format("<color=blue>{0}</color> attacked <color=red>{1}</color>, but <color=red>{1}</color> defended themselves.",
            attacker.name,
            target.name
        ));
    }
}