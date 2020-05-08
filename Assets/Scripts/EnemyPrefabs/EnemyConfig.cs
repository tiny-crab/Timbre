using System;
using System.Collections.Generic;

public interface EnemyConfig {
    string name {get;}
    List<Behavior> behaviors {get;}
    List<Behavior> fearBehaviors {get;}
    Dictionary<string, string> flavorText {get;}
}

public static class EnemyConfigUtils {
    public static EnemyConfig ToEnemyConfig(this string enemyConfigName) {
        return enemyConfigNameToEnemyConfig[enemyConfigName];
    }

    public static Dictionary<string, EnemyConfig> enemyConfigNameToEnemyConfig =
        new Dictionary<string, EnemyConfig>() {
                {"ShamblingCorpse", new ShamblingCorpse()},
                {"ElkCultCaster", new ElkCultCaster()}
        };
}

public class ShamblingCorpse : EnemyConfig {
    public string name { get {
        return "Shambling Corpse";
    } }

    public List<Behavior> behaviors { get {
        return new List<Behavior> {
            new MeleeAttackV1()
        };
    } }

    public List<Behavior> fearBehaviors { get {
        return new List<Behavior> {
            new Flee()
        };
    } }

    public Dictionary<string, string> flavorText { get {
        return new Dictionary<string, string> {
            {typeof(MeleeAttackV1).Name, $"{name} struck out with a powerful forelimb!"},
            {typeof(Flee).Name, $"{name} stumbled away in fear!"}
        };
    } }
}

public class ElkCultCaster : EnemyConfig {
    public string name { get {
        return "Elken Spellcaster";
    } }

    public List<Behavior> behaviors { get {
        return new List<Behavior> {
            new RangedAttackV1()
        };
    } }

    public List<Behavior> fearBehaviors { get {
        return new List<Behavior> {
            new EvasiveTeleport()
        };
    } }

    public Dictionary<string, string> flavorText { get {
        return new Dictionary<string, string> {
            {typeof(RangedAttackV1).Name, $"{name} conjured a bolt of green energy!"},
            {typeof(EvasiveTeleport).Name, $"{name} becomes enwreathed with a green glow."},
            {typeof(Flee).Name, $"{name} ran away in fear!"}
        };
    } }
}