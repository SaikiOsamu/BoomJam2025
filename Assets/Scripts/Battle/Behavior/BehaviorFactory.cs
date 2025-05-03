using System;
using System.Collections.Generic;
using UnityEngine;
using static MovementHandler2D;

public interface Behavior
{
    BattleEntity.MoveDelegate MoveDelegate { get; }
    BattleEntity.AttackDelegate AttackDelegate { get; }
    BattleEntity.CollideDelegate CollideDelegate { get; }
    BattleEntity.SelfDestructDelegate SelfDestructDelegate { get; }
}

public class BaseBehavior : Behavior
{
    public virtual BattleEntity.MoveDelegate MoveDelegate { get => _ => Vector2.zero; }
    public virtual BattleEntity.AttackDelegate AttackDelegate { get => _ => new List<BattleEntity>(); }
    public virtual BattleEntity.CollideDelegate CollideDelegate { get => (_, _) => false; }
    public virtual BattleEntity.SelfDestructDelegate SelfDestructDelegate { get => _ => { }; }

    public MovementState moveState = new MovementState();
}

public class BehaviorFactory
{
    public static Behavior GetBehavior(BehaviorDefinitions definitions)
    {
        switch (definitions.behaviorType)
        {
            case BehaviorType.DamagingProjectile:
                return new DamagingProjectileBehavior(definitions);
            case BehaviorType.Player:
                return new PlayerBehavior(definitions);
            case BehaviorType.Barrier:
                return new BarrierBehavior(definitions);
            case BehaviorType.Blink:
                return new BlinkBehavior(definitions);
            case BehaviorType.Enemy:
                return new RangedEnemyBehavior(definitions);
            case BehaviorType.GhostEnemy:
                return new GhostEnemyBehavior(definitions);
            case BehaviorType.IronFist:
                return new IronFistBehavior(definitions);
            case BehaviorType.GrandWitch:
                return new GrandWitchBehavior(definitions);
            case BehaviorType.Bird:
                return new BombBirdBehavior(definitions);
            case BehaviorType.FloatingCannon:
                return new FloatingCannonBehavior(definitions);
            case BehaviorType.Naofu:
                return new NaofuBehavior(definitions);
            case BehaviorType.Charger:
                return new ChargerBehavior(definitions);
            case BehaviorType.Rabbit:
                return new RabbitBehavior(definitions);
            case BehaviorType.Cobra:
                return new CobraBehavior(definitions);
            case BehaviorType.Turtle:
                return new TurtleBehavior(definitions);
            case BehaviorType.Unknown:
            default:
                return new BaseBehavior();
        }
    }
}
