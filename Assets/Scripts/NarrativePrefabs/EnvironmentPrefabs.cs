using System.Collections.Generic;

public abstract class EnvironmentNarrativePrefab {
    public int counter = 0;
    public abstract List<List<string>> potentialDialogues {get;}
    public List<string> getNextMessage() {
        var toReturn = potentialDialogues[counter];
        counter++;
        if (counter == potentialDialogues.Count) { counter = 0; }
        return toReturn;
    }
}

public static class EnvironmentNarrativePrefabUtils {
    public static Dictionary<string, EnvironmentNarrativePrefab> narrativePrefabNameToNarrativePrefab = new Dictionary<string, EnvironmentNarrativePrefab>() {
            {"ColdWindNarrative", new ColdWindNP()},
            {"BloodyTentNarrative", new BloodyTentNP()},
            {"EinarsAxeNarrative", new EinarsAxeNP()},
            {"EinarBridgeShiverNarrative", new EinarBridgeShiverNP()},
            {"EvelynSightingNarrative", new EvelynSightingNP()},
            {"DistortedHummingIntroNarrative", new DistortedHummingIntroNP()},
            {"EvelynDeepBreathNarrative", new EvelynDeepBreathNP()},
        };

    public static EnvironmentNarrativePrefab ToDialoguePrefab(this string dialoguePrefabName) {
        return narrativePrefabNameToNarrativePrefab[dialoguePrefabName];
    }
}

public class ColdWindNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "A cold wind blows through the forest corridor." }
        };
    }}
}

public class BloodyTentNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "The stench of blood permeates the tent." }
        };
    }}
}

public class EinarsAxeNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "An axe against wood can be heard in the distance." }
        };
    }}
}

public class EinarBridgeShiverNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "Einar shivers briefly as he crosses the bridge." }
        };
    }}
}

public class EvelynSightingNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "A knight in layered armor stands in the glade, alone." }
        };
    }}
}

public class DistortedHummingIntroNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "Distorted humming fills the air..." }
        };
    }}
}

public class EvelynDeepBreathNP : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "Evelyn lets out a deep breath, and stares intensely into the distance." }
        };
    }}
}