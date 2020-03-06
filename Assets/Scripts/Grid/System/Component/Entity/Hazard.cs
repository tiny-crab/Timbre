public abstract class Hazard {
    abstract public void OnEntityContact(GridEntity entity);
}

public class Caltrops : Hazard {
    public Caltrops () {}

    override public void OnEntityContact(GridEntity entity) {
        entity.TakeDamage(1);
    }
}

public class BearTrap : Hazard {
    public BearTrap () {}

    override public void OnEntityContact(GridEntity entity) {
        if (entity.isFriendly || entity.isAllied) {
            // disappear if ally touches it

        }
        else {
            // damage and immobilize an enemy
            entity.TakeDamage(1);
            var maxMoves = entity.maxMoves;
            entity.maxMoves = 0;
            entity.overrides.Add(new GridEntity.Override(
                "BearTrap",
                1,
                () => {
                    entity.maxMoves = maxMoves;
                    return true;
                }
            ));
        }
    }
}
