using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface Skill {
    int cost {get;}
}

public abstract class SelectTilesSkill : Skill {
    public abstract int cost {get;}
    public abstract int radius {get;}
    public abstract int targets {get;}
    public abstract void ResolveEffect(GridEntity source, Tile tile);
    public virtual List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return GridUtils.GenerateTileCircle(grid, radius, sourceTile);
    }
}

public abstract class SelectEnemiesSkill : SelectTilesSkill {
    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return GridUtils
                .GenerateTileCircle(grid, radius, sourceTile)
                .Where(tile => tile.occupier != null && tile.occupier.isHostile)
                .ToList();
    }
}

public abstract class SelectAlliesSkill : SelectTilesSkill {
    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return GridUtils
                .GenerateTileCircle(grid, radius, sourceTile)
                .Where(tile => tile.occupier != null && (tile.occupier.isAllied || tile.occupier.isFriendly))
                .ToList();
    }
}

public abstract class BuffSkill : Skill {
    public abstract int cost {get;}
    public abstract void ResolveEffect(GridEntity source);
}

public abstract class AttackSkill : Skill {
    public abstract int cost {get;}
    public abstract void BeforeAttack(GridEntity attacker, GridEntity target);
    public abstract void AfterAttack(GridEntity attacker, GridEntity target);
}

public static class SkillUtils {
    public static Dictionary<string, Skill> skillNameToSkill = new Dictionary<string, Skill>() {
            {"Caltrops", new CaltropsSkill()},
            {"Defend Self", new DefendSelf()},
            {"Headshot", new Headshot()},
            {"HitAndRun", new HitAndRun()},
            {"Maul", new Headshot()},
            {"Protect Ally", new ProtectAlly()},
            {"Retaliate", new Retaliate()},
            {"Revive", new Revive()}
        };

    public static List<Skill> ToSkills(this List<string> skillNames)
        { return skillNames.Select(name => skillNameToSkill[name]).ToList(); }

    public enum GambleOdds {
        COIN,
        D4,
        D6
    }

    public static bool Gamble(GambleOdds odds) {
        int target = 0;
        switch (odds) {
            case GambleOdds.COIN:
                target = 2;
                break;
            case GambleOdds.D4:
                target = 4;
                break;
            case GambleOdds.D6:
                target = 6;
                break;
            default:
                target = 10;
                break;
        }
        var random = new System.Random().Next(0, target);
        Debug.Log("You rolled a " + random);

        return random == target-1;
    }
}

// TILE SELECT SKILLS

public class CaltropsSkill : SelectTilesSkill {
    public override int cost { get { return 1; } }
    public override int radius { get { return 2; } }
    public override int targets { get { return 2; } }

    override public void ResolveEffect(GridEntity source, Tile tile) {
        tile.hazards.Add(new Caltrops());
    }

    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return base.GetValidTiles(grid, sourceTile).Where(tile => tile.occupier == null).ToList();
    }
}

public class DefendSelf : SelectTilesSkill {
    public override int cost { get { return 1; } }
    public override int radius { get { return 1; } }
    public override int targets { get { return 1; } }

    override public void ResolveEffect(GridEntity source, Tile tile) {
        source.currentReactions.Add(new DefendSelfReaction(new List<Tile>() {tile}, source));
        // skill immediately "ends" the turn of the entity using it.
        source.ConsumeTurnResources();
    }
}

// ENEMY SELECT SKILLS

// ALLY SELECT SKILLS

public class Revive : SelectAlliesSkill {
    public override int cost { get { return 1; } }
    public override int radius { get { return 1; } }
    public override int targets { get { return 1; } }

    override public void ResolveEffect(GridEntity source, Tile tile) {
        var entity = tile.occupier;
        entity.currentHP = entity.maxHP;
        entity.currentMoves = Mathf.FloorToInt( (float) entity.maxMoves / 2);
        entity.currentAttacks = 0;
        entity.currentSP = 0;
        entity.outOfHP = false;
    }

    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return base.GetValidTiles(grid, sourceTile).Where(tile => tile.occupier.outOfHP).ToList();
    }
}

public class ProtectAlly : SelectAlliesSkill {
    public override int cost { get { return 1; } }
    public override int radius { get { return 1; } }
    public override int targets { get { return 1; } }

    override public void ResolveEffect(GridEntity source, Tile tile) {
        var entity = tile.occupier;
        entity.damageReceiver = source;
        // skill immediately "ends" the turn of the entity using it.
        source.ConsumeTurnResources();
        // skill immediately freezes the entity next to it
        entity.currentMoves = 0;
        entity.outOfMoves = true;

        bool ProtectAllyOverride() {
            entity.damageReceiver = entity;
            return true;
        }

        entity.overrides.Add(new GridEntity.Override(0, ProtectAllyOverride));
    }
}

// BUFF SKILLS
public class Retaliate : BuffSkill {
    public override int cost { get { return 1; } }

    override public void ResolveEffect(GridEntity source) {
        source.currentReactions.Add(new RetaliateReaction(source));
    }
}


// ATTACKING SKILLS
public class Headshot : AttackSkill {
    public override int cost { get { return 2; } }

    override public void BeforeAttack(GridEntity attacker, GridEntity target) {
        if (SkillUtils.Gamble(SkillUtils.GambleOdds.D4)) {
            attacker.damageMult = 3;
        }
    }

    override public void AfterAttack(GridEntity attacker, GridEntity target) {
        attacker.damageMult = 1;
    }
}

public class HitAndRun : AttackSkill {
    public override int cost { get { return 1; } }

    override public void BeforeAttack(GridEntity attacker, GridEntity target) {
        // do nothing
    }

    override public void AfterAttack(GridEntity attacker, GridEntity target) {
        attacker.currentMoves += attacker.maxMoves * 2;
    }
}