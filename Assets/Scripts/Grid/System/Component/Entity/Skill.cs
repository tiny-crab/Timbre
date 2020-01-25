using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface Skill {
    int level {get; set;}
    string name {get;}
    string desc {get;}
    int cost {get;}
}

public abstract class SelectTilesSkill : Skill {
    protected int levelVal;
    public int level {
        get { return levelVal; }
        set { levelVal = value; }
    }
    public abstract string name {get;}
    public abstract string desc {get;}

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
    protected int levelVal;
    public int level {
        get { return levelVal; }
        set { levelVal = value; }
    }
    public abstract string name {get;}
    public abstract string desc {get;}

    public abstract int cost {get;}
    public abstract void ResolveEffect(GridEntity source);
}

public abstract class AttackSkill : Skill {
    protected int levelVal;
    public int level {
        get { return levelVal; }
        set { levelVal = value; }
    }
    public abstract string name {get;}
    public abstract string desc {get;}

    public abstract int cost {get;}
    public abstract void BeforeAttack(GridEntity attacker, GridEntity target);
    public abstract void AfterAttack(GridEntity attacker, GridEntity target);
}

public static class SkillUtils {
    public static Dictionary<string, Skill> skillNameToSkill = new Dictionary<string, Skill>() {
            {"Brambles", new Brambles()},
            {"Caltrops", new CaltropsSkill()},
            {"Defend Self", new DefendSelf()},
            {"Headshot", new Headshot()},
            {"HitAndRun", new HitAndRun()},
            {"Maul", new Maul()},
            {"Protect Ally", new ProtectAlly()},
            {"Retaliate", new Retaliate()},
            {"Revive", new Revive()}
        };

    public static List<Skill> ToSkills(this List<string> skillNames) {
        return skillNames.Select(name => skillNameToSkill[name]).ToList();
    }

    public static List<Skill> PopulateSkills(List<string> skillNames, List<int> levels) {
        var skills = skillNames.ToSkills();
        skills.Each((skill, index) => skill.level = levels[index]);
        return skills.Where(skill => skill.level != 0).ToList();
    }

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
    public override string name { get {
        return "Caltrops";
    } }
    public override string desc { get {
        return "Target: Spaces within 2 tiles" +
        "\n\tScatter spikes on tiles nearby. Does 1 damage when stepped on.";
    } }

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
    public override string name { get {
        return "Defend Self";
    } }
    public override string desc { get {
        return "Target: Adjacent tile" +
        "\n\tNullify all ranged attacks and melee attacks from the selected tile";
    } }

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

public class Brambles : SelectEnemiesSkill {
    public override string name { get {
        return "Brambles";
    } }
    public override string desc { get {
        return "Target: Enemy in combat" +
        "\n\tImmobilize an enemy for one turn.";
    } }

    public override int cost { get { return 1; } }
    public override int radius { get { return int.MaxValue; } }
    public override int targets { get { return 1; } }

    override public void ResolveEffect(GridEntity source, Tile tile) {
        var maxMoves = tile.occupier.maxMoves;
        tile.occupier.maxMoves = 0;
        tile.occupier.overrides.Add(new GridEntity.Override(1, () => {
            tile.occupier.maxMoves = maxMoves;
            return true;
        }));
    }

    override public List<Tile> GetValidTiles(GameObject[,] grid, Tile sourceTile) {
        return base.GetValidTiles(grid, sourceTile);
    }
}

// ALLY SELECT SKILLS

public class Revive : SelectAlliesSkill {
    public override string name { get {
        return "Revive";
    } }
    public override string desc { get {
        return "Target: Adjacent ally with 0 HP" +
        "\n\tBring an ally back into the fray.";
    } }

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
    public override string name { get {
        return "Protect Ally";
    } }
    public override string desc { get {
        return "Target: Adjacent ally" +
        "\n\tAbsorb damage on behalf of the selected ally.";
    } }

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

        entity.overrides.Add(new GridEntity.Override(0, () => {
            entity.damageReceiver = entity;
            return true;
        }));
    }
}

// BUFF SKILLS
public class Retaliate : BuffSkill {
    public override string name { get {
        return "Retaliate";
    } }
    public override string desc { get {
        return "Triggered: When melee attacked by enemy" +
        "\n\tDouble damage after being attacked.";
    } }

    public override int cost { get { return 1; } }

    override public void ResolveEffect(GridEntity source) {
        source.currentReactions.Add(new RetaliateReaction(source));
    }
}


// ATTACKING SKILLS
public class Headshot : AttackSkill {
    public override string name { get {
        return "Headshot";
    } }
    public override string desc { get {
        return "Target: Enemy in attack range" +
        "\n\t25% chance to do triple damage.";
    } }

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
    public override string name { get {
        return "Hit and Run";
    } }
    public override string desc { get {
        return "Target: Enemy in attack range" +
        "\n\tRegain double your max movement after attacking an enemy.";
    } }

    public override int cost { get { return 1; } }

    override public void BeforeAttack(GridEntity attacker, GridEntity target) {
        // do nothing
    }

    override public void AfterAttack(GridEntity attacker, GridEntity target) {
        attacker.currentMoves += attacker.maxMoves * 2;
    }
}

public class Maul : AttackSkill {
    public override string name { get {
        return "Maul";
    } }
    public override string desc { get {
        return "Target: Enemy in attack range" +
        "\n\t50% chance to make enemy instantly afraid.";
    } }

    public override int cost { get { return 1; } }

    override public void BeforeAttack(GridEntity attacker, GridEntity target) {
        // do nothing
    }

    override public void AfterAttack(GridEntity attacker, GridEntity target) {
        if (SkillUtils.Gamble(SkillUtils.GambleOdds.COIN)) {
            target.baseFearValue = target.fearThreshold;
        }
    }
}