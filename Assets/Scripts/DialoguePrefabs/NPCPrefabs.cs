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
            {"RamdogDialogue", new RamdogDialoguePrefab()},
            {"SuspiciousDialogue", new SuspiciousDialoguePrefab()}
        };

    public static NPCDialoguePrefab ToDialoguePrefab(this string dialoguePrefabName) {
        return dialoguePrefabNameToDialoguePrefab[dialoguePrefabName];
    }
}

public class RamdogDialoguePrefab : NPCDialoguePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "I like your ramdog.", "Is he purebred?" },
            new List<string>{ "I got him some jerky.", "It's made by the butcher down the street.", "I hope he likes it!" }
        };
    }}
}

public class SuspiciousDialoguePrefab : NPCDialoguePrefab {
    public override List<List<string>> potentialDialogues { get {
        return new List<List<string>> {
            new List<string>{ "What are you looking at?" },
            new List<string>{ "I don't like your demeanor.", "Can you please give me a little space?" },
            new List<string>{ "Like I said, please leave me alone." },
            new List<string>{ "Back off." },
        };
    }}
}