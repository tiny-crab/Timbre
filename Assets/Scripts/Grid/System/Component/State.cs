using System.Collections.Generic;
using System.Linq;

public abstract class State {
    public GridEntity source;
}

public class NoSelectionState : State {
    public NoSelectionState() {}
}

public class AllySelectedState : State {
    public List<Tile> moveRange;
    public List<Tile> attackRange;
    public Skill lastActivatedSkill;

    public AllySelectedState(GridEntity selectedEntity) {
        source = selectedEntity;
    }
}

public class EnemySelectedState : State {
    public Dictionary<TileAction, double> tileScoreMap;
    public Behavior bestBehavior;

    public EnemySelectedState(GridEntity selectedEntity) {
        source = selectedEntity;
    }
}

public class SelectSkillActivatedState : State {
    public SelectTilesSkill activeSkill;
    public List<Tile> validTiles = new List<Tile>();
    public List<Tile> selectedTiles = new List<Tile>();

    public SelectSkillActivatedState(GridEntity source, SelectTilesSkill activated, List<Tile> validTiles) {
        this.source = source;
        activeSkill = activated;
        this.validTiles = validTiles;
    }
}

public class TeleportActivatedState : State {
    public List<Tile> validTiles;

    public TeleportActivatedState(GridEntity source, List<Tile> validTiles) {
        this.source = source;
        this.validTiles = validTiles;
    }
}

public class EnemyTurnState : State {
    public List<GridEntity> enemies;
    public List<IGrouping<GridEntity, Behavior>> aiSteps;

    public EnemyTurnState(List<GridEntity> enemies) {
        this.enemies = enemies;
    }
}
