using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public string narrativePrefabName;
    public Dialog narrativeDialog;

    private EnvironmentNarrativePrefab narrativePrefab;

    public void Start() {
        narrativeDialog = (Dialog) GameObject.Find("NarrativeDialog").GetComponent<Dialog>();
        narrativePrefab = EnvironmentNarrativePrefabUtils.ToDialoguePrefab(narrativePrefabName);
    }

    public void Trigger() {
        narrativeDialog.PostToDialog(narrativePrefab.getNextMessage(), pitched: false);
    }
}
