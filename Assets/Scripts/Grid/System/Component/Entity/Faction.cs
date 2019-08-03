using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction {

    public string name;
    public List<GridEntity> entities = new List<GridEntity>();
    public bool isPlayerFaction;
    public bool isHostileFaction;

    public bool OutOfResources () {
        // todo add computation
        return isHostileFaction;
    }

    public Faction(string name, bool isPlayerFaction, params GridEntity[] entities) {
        this.name = name;
        this.entities.AddRange(entities);
        isHostileFaction = !isPlayerFaction;
        this.isPlayerFaction = isPlayerFaction;
        if(isPlayerFaction) { this.entities.ForEach(entity => entity.isAllied = true ); }
        if(isHostileFaction) { this.entities.ForEach(entity => entity.isHostile = true); }
    }

    public void RefreshTurnResources() {
        foreach (var entity in entities) { entity.RefreshTurnResources(); };
    }
}
