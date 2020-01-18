using System;
using UnityEngine;

public abstract class EthreadEffect {
    public abstract void ApplyEffect(GridEntity entity);
    public abstract void RemoveEffect(GridEntity entity);
}

public class RedEthreadEffect : EthreadEffect {
    public override void ApplyEffect(GridEntity entity) {
        entity.damage++;
    }

    public override void RemoveEffect(GridEntity entity) {
        entity.damage--;
    }
}