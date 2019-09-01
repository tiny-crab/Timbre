using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SelectTilesSkill {
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

public abstract class AttackSkill {}

public static class SkillUtils {
    public static Dictionary<string, SelectTilesSkill> skillNameToSkill = new Dictionary<string, SelectTilesSkill>() {
            {"Caltrops", new CaltropsSkill()},
            {"Defend Self", new Revive()},
            {"Headshot", new Revive()},
            {"Protect Ally", new Revive()},
            {"Retaliate", new Revive()},
            {"Revive", new Revive()}
        };

    public static List<SelectTilesSkill> ToSkills(this List<string> skillNames)
        { return skillNames.Select(name => skillNameToSkill[name]).ToList(); }
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
