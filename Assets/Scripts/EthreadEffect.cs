using System;
using UnityEngine;

public abstract class EthreadEffect {
    public abstract void ApplyEffect(GridEntity entity);
}

public class DamageEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.damage++; }
}

public class MoveEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.maxMoves++; }
}

public class SPEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.maxSP++; }
}

public class PrimarySkillEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.skillLevels[0]++;}
}

public class SecondarySkillEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.skillLevels[1]++;}
}

public class TertiarySkillEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) { entity.skillLevels[2]++;}
}