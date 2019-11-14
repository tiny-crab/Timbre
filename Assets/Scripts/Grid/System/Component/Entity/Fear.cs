using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Fear {
    public abstract int CalculateFear(GridEntity entity);
}

public static class FearUtils {
    public static Dictionary<string, Fear> fearNameToFear =
        new Dictionary<string, Fear>() {
            { "Damage", new Damage() }
        };

    public static List<Fear> ToFears(this List<string> fearNames)
        { return fearNames.Select(name => {
            var fear = fearNameToFear[name];
            return fear;
        }).ToList(); }
}

public class Damage: Fear {
    public override int CalculateFear(GridEntity entity) {
        if (entity.currentHP == 0) { return 0; }
        return Mathf.CeilToInt(Mathf.Pow(1 / ( (float) entity.currentHP / entity.maxHP), 2));
    }
}