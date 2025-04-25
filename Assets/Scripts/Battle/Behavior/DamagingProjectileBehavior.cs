using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class DamagingProjectileBehavior : BaseBehavior
{
    public override AttackDelegate AttackDelegate => MaybeDoSkill;
    public override CollideDelegate CollideDelegate => Collide;
    public override SelfDestructDelegate SelfDestructDelegate => param =>
    {
        maybeTimeBasedDelegate.Invoke(param);
        if ((param.entity.position - param.player.position).magnitude > 500)
        {
            param.entity.isAlive = false;
        }
        if (!param.entity.isAlive && !shouldDoFinaleSkill && definitions.doSkillWhenDisappear)
        {
            // Medical miracle!
            param.entity.isAlive = true;
            shouldDoFinaleSkill = true;
        }
    };

    BehaviorDefinitions definitions;

    private CollideDelegate mCollideDelegate;
    private SelfDestructDelegate maybeTimeBasedDelegate = _ => { };

    bool shouldDoFinaleSkill = false;
    public List<BattleEntity> MaybeDoSkill(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (shouldDoFinaleSkill)
        {
            param.entity.isAlive = false;
            var summonedObjects = param.entity.GetSkillSummon(0, out _);
            foreach (var obj in summonedObjects)
            {
                if (obj.facingEast)
                {
                    obj.position += definitions.disappearSkillPositionOffset;
                }
                else
                {
                    obj.position.x -= definitions.disappearSkillPositionOffset.x;
                    obj.position.y += definitions.disappearSkillPositionOffset.y;
                }
            }
            result.AddRange(summonedObjects);
        }
        return result;
    }

    public bool Collide(EntityUpdateParams param, BattleEntity other)
    {
        bool collideHappened = mCollideDelegate.Invoke(param, other);
        // Apply status.
        if (collideHappened)
        {
            foreach (BattleStatusEffect eff in definitions.grantingStatus)
            {
                BattleStatus status = new BattleStatus();
                status.status = eff;
                status.pushBackFacingEast = param.entity.facingEast;
                status.pullCenter = param.entity.position;
                other.statusInEffect.Add(status);
            }
        }
        return collideHappened;
    }

    public DamagingProjectileBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        mCollideDelegate = new AttackCollideHandler(
            definitions.maxDamageTargets, definitions.projectileDamage,
            definitions.projectileDamageEveryNSecond).Update;
        if (definitions.timeToLive > 0)
        {
            maybeTimeBasedDelegate = new TimedProjectionSelfDestructHandler(definitions.timeToLive).Update;
        }
    }
}