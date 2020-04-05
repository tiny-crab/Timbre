public abstract class Hazard {
    public abstract string name {get;}
    protected bool deployedVal;
    public bool deployed {
        get { return deployedVal; }
        set { deployedVal = value;}
    }
    public abstract int uses {get;}
    protected int triggeredVal;
    public int triggered {
        get { return triggeredVal; }
        set { triggeredVal = value; }
    }
    abstract public void OnEntityContact(GridEntity entity);
}

public class Caltrops : Hazard {
    public override string name { get {
        return "Caltrops";
    } }
    public override int uses { get {
        return 1;
    } }

    override public void OnEntityContact(GridEntity entity) {
        entity.TakeDamage(1);
    }
}

public class BearTrap : Hazard {
    public override string name { get {
        return "BearTrap";
    } }
    public override int uses { get {
        return 1;
    } }

    override public void OnEntityContact(GridEntity entity) {
        if (entity.isFriendly || entity.isAllied) {
            // disappear if ally touches it
            triggered++;
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
            triggered++;
        }
    }
}
