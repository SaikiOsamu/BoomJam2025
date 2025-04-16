using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class DamagingProjectile : BaseBehavior
{
    public override CollideDelegate CollideDelegate => mCollideDelegate;
    public override SelfDestructDelegate SelfDestructDelegate => param =>
    {
        maybeTimeBasedDelegate.Invoke(param);
        if ((param.entity.position - param.player.position).magnitude > 500)
        {
            param.entity.isAlive = false;
        }
    };

    BehaviorDefinitions definitions;

    private CollideDelegate mCollideDelegate;
    private SelfDestructDelegate maybeTimeBasedDelegate = _ => { };

    public DamagingProjectile(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        mCollideDelegate = new AttackCollideHandler(definitions.maxDamageTargets, definitions.projectileDamage).Update;
        if (definitions.timeToLive > 0)
        {
            maybeTimeBasedDelegate = new TimedProjectionSelfDestructHandler(definitions.timeToLive).Update;
        }
    }
}