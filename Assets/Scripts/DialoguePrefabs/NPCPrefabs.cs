using System.Collections.Generic;

public abstract class NPCDialoguePrefab {
    public int counter = 0;
    public abstract List<List<string>> potentialDialogues {get;}
    public List<string> getNextMessage() {
        var toReturn = potentialDialogues[counter];
        counter++;
        if (counter == potentialDialogues.Count) { counter = 0; }
        return toReturn;
    }
}

public static class NPCDialoguePrefabUtils {
    public static Dictionary<string, NPCDialoguePrefab> dialoguePrefabNameToDialoguePrefab = new Dictionary<string, NPCDialoguePrefab>() {
            {"RamdogDialogue", new RamdogDialoguePrefab()}
        };

    public static NPCDialoguePrefab ToDialoguePrefab(this string dialoguePrefabName) {
        return dialoguePrefabNameToDialoguePrefab[dialoguePrefabName];
    }
}

public class RamdogDialoguePrefab : NPCDialoguePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "I like your ramdog.", "Is he purebred?" },
            new List<string>{ "I got him some jerky.", "It's made by the butcher down the street." }
        };
    }}
}