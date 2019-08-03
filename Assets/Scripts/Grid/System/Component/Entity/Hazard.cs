public abstract class Hazard {
    abstract public void OnEntityContact(GridEntity entity);
}

public class Caltrops : Hazard {
    public Caltrops () {}

    override public void OnEntityContact(GridEntity entity) {
        entity.TakeDamage(1);
    }
}
