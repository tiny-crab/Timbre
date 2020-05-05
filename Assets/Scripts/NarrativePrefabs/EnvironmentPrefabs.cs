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
            {"ColdWindNarrative", new ColdWindNarrativePrefab()}
        };

    public static EnvironmentNarrativePrefab ToDialoguePrefab(this string dialoguePrefabName) {
        return narrativePrefabNameToNarrativePrefab[dialoguePrefabName];
    }
}

public class ColdWindNarrativePrefab : EnvironmentNarrativePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "A cold wind blows through the forest corridor." }
        };
    }}
}