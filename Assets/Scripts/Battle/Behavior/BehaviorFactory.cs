using System;
using System.Collections.Generic;
using UnityEngine;

public interface Behavior
{
    BattleEntity.MoveDelegate MoveDelegate { get; }
     BattleEntity.AttackDelegate AttackDelegate { get; }
     BattleEntity.CollideDelegate CollideDelegate { get;}
     BattleEntity.SelfDestructDelegate SelfDestructDelegate { get; }
}

public class BaseBehavior : Behavior
{
    public virtual BattleEntity.MoveDelegate MoveDelegate { get => _ => Vector2.zero; }
    public virtual BattleEntity.AttackDelegate AttackDelegate { get => _ => new List<BattleEntity>(); }
    public virtual BattleEntity.CollideDelegate CollideDelegate { get => (_, _) => { }; }
    public virtual BattleEntity.SelfDestructDelegate SelfDestructDelegate { get => _ => { }; }
}

public class BehaviorFactory
{
    public static Behavior GetBehavior(BehaviorDefinitions definitions)
    {
        switch (definitions.behaviorType)
        {
            case BehaviorType.Enemy:
                return new GeneralEnemyBehavior(definitions);
            case BehaviorType.Bird:
                return new BombBirdBehavior(definitions);
            case BehaviorType.Naofu:
                return new NaofuBehavior(definitions);
            case BehaviorType.Unknown:
            default:
                return new BaseBehavior();
        }
    }
}
