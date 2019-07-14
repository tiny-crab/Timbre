using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Faction {

    public List<GridEntity> entities = new List<GridEntity>();
    public string name;

    public Faction(string name, params GridEntity[] entities) {
        this.name = name;
        this.entities.AddRange(entities);
    }

}