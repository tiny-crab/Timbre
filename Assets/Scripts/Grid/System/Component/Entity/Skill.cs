using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface Skill {}

public abstract class SelectTilesSkill : Skill {
    public abstract int radius {get;}
    public abstract int targets {get;}
    public abstract void ResolveEffect(Tile tile);
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

public abstract class AttackSkill : Skill {
    public abstract void BeforeAttack(GridEntity attacker, GridEntity target);
    public abstract void AfterAttack(GridEntity attacker, GridEntity target);
}

public static class SkillUtils {
    public static Dictionary<string, Skill> skillNameToSkill = new Dictionary<string, Skill>() {
            {"Caltrops", new CaltropsSkill()},
            {"Defend Self", new Revive()},
            {"Headshot", new Headshot()},
            {"Protect Ally", new Revive()},
            {"Retaliate", new Revive()},
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
    public override int radius { get { return 2; } }
    public override int targets { get { return 2; } }

    override public void ResolveEffect(Tile tile) {
        tile.hazards.Add(new Caltrops());
    }

    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return base.GetValidTiles(grid, sourceTile).Where(tile => tile.occupier == null).ToList();
    }
}

// ENEMY SELECT SKILLS

// ALLY SELECT SKILLS

public class Revive : SelectAlliesSkill {
    public override int radius { get { return 1; } }
    public override int targets { get { return 1; } }

    override public void ResolveEffect(Tile tile) {
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

// ATTACKING SKILLS
public class Headshot : AttackSkill {
    override public void BeforeAttack(GridEntity attacker, GridEntity target) {
        if (SkillUtils.Gamble(SkillUtils.GambleOdds.D4)) {
            attacker.damageMult = 3;
        }
    }

    override public void AfterAttack(GridEntity attacker, GridEntity target) {
        attacker.damageMult = 1;
    }
}