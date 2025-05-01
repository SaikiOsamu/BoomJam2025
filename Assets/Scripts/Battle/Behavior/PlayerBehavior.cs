using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

internal class SkillCooldownAndActionPair
{
    public InputAction action;
    public float castTime;
    public bool hasCasted = false;

    public SkillCooldownAndActionPair(InputAction action)
    {
        this.action = action;
        castTime = 0;
    }
}

class PlayerBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => Update;

    BehaviorDefinitions definitions;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction barrierAction;
    InputAction blinkAction;
    List<float> skillCooldown = new();
    List<SkillCooldownAndActionPair> skillCooldownAndActionPairs = new();

    public PlayerBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        moveAction = definitions.moveAction.action;
        jumpAction = definitions.jumpAction.action;
        blinkAction = definitions.blinkAction.action;
        attackAction = definitions.attackAction.action;
        barrierAction = definitions.barrierAction.action;
        for (int i = 0; i < 20; i++)
        {
            skillCooldown.Add(0);
        }
        skillCooldownAndActionPairs.Add(new SkillCooldownAndActionPair(definitions.skill1Action.action));
        skillCooldownAndActionPairs.Add(new SkillCooldownAndActionPair(definitions.skill2Action.action));
        skillCooldownAndActionPairs.Add(new SkillCooldownAndActionPair(definitions.skill3Action.action));
        skillCooldownAndActionPairs.Add(new SkillCooldownAndActionPair(definitions.skill4Action.action));
    }

    private List<BattleEntity> ActivateSkill(int skillIndex, BattleEntity entity, bool dynamic = false)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (skillCooldown[skillIndex] > 0)
        {
            return result;
        }
        else
        {
            var entitiesSummoned =
                entity.GetSkillSummon(skillIndex, out float cooldown, out int godPowerConsumption, dynamic);
            if (entity.godPower >= godPowerConsumption)
            {
                entity.godPower -= godPowerConsumption;
            }
            else
            {
                return result;
            }
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                toSummon.facingEast = entity.facingEast;
                if (entity.facingEast)
                {
                    toSummon.position.x += 0.4f;
                }
                else
                {
                    toSummon.position.x -= 0.4f;
                }
                result.Add(toSummon);
            }
            skillCooldown[skillIndex] = cooldown;
        }
        if (entity.GetSkill(skillIndex, dynamic).skillName.Equals(entity.GetSkill(1, false).skillName))
        {
            foreach (BattleEntity e in result)
            {
                e.moveHandler = new FlyingSwordMoveHandler(entity).Move;
            }
        }
        if (entity.GetSkill(skillIndex, dynamic).skillName.Equals(entity.GetSkill(4, false).skillName))
        {
            foreach (BattleEntity e in result)
            {
                // Maybe throw it out
                if (e.prefabCharacter != null && e.prefabCharacter.behavior.moveSpeed != 0)
                {
                    e.moveHandler = new VelocityMoveHandler(
                        e.prefabCharacter.behavior.moveSpeed,
                        new Vector2(entity.facingEast ? 1 : -1, 0).normalized).Move;
                }
            }
        }
        if (entity.GetSkill(skillIndex, dynamic).skillName.Equals(entity.GetSkill(5, false).skillName))
        {
            foreach (BattleEntity e in result)
            {
                // Attach fire spread ability.
                FireSpreadBehavior spreadRight = new FireSpreadBehavior();
                FireSpreadBehavior spreadLeft = new FireSpreadBehavior();
                spreadLeft.facingEast = false;
                e.attackHandler = param =>
                {
                    List<BattleEntity> result = new List<BattleEntity>();
                    result.AddRange(spreadRight.Spread(param));
                    result.AddRange(spreadLeft.Spread(param));
                    return result;
                };
            }
        }
        if (entity.GetSkill(skillIndex, dynamic).skillName.Equals(entity.GetSkill(7, false).skillName))
        {
            foreach (BattleEntity e in result)
            {
                // Mark time extender ultimate.
                e.isTimeExtender = true;
            }
        }
        if (entity.GetSkill(skillIndex, dynamic).skillName.Equals(entity.GetSkill(8, false).skillName))
        {
            foreach (BattleEntity e in result)
            {
                e.attackHandler = new LightningControllerBehavior().MaybeLightning;
            }
        }
        return result;
    }

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        for (int i = 0; i < 20; ++i)
        {
            if (skillCooldown[i] > 0)
            {
                skillCooldown[i] -= param.timeDiff;
            }
        }

        if (attackAction.triggered)
        {
            result.AddRange(ActivateSkill(0, param.entity));
        }
        for (int i = 0; i < skillCooldownAndActionPairs.Count; i++)
        {
            SkillCooldownAndActionPair skill = skillCooldownAndActionPairs[i];
            if (skill.action.triggered)
            {
                if (param.entity.GetSkillCasttime(i, true) <= 0)
                {
                    skill.hasCasted = true;
                    result.AddRange(ActivateSkill(i, param.entity, true));
                }
                else
                {
                    skill.castTime += param.timeDiff;
                }
            }
            else if (skill.action.IsPressed())
            {
                if (!skill.hasCasted)
                {
                    skill.castTime += param.timeDiff;
                    if (param.entity.GetSkillCasttime(i, true) < skill.castTime)
                    {
                        skill.hasCasted = true;
                        result.AddRange(ActivateSkill(i, param.entity, true));
                    }
                }
            }
            else
            {
                skill.castTime = 0;
                skill.hasCasted = false;
            }
        }
        if (barrierAction.triggered && param.entity.position.y <= 0)
        {
            var barrier = ActivateSkill(2, param.entity);
            foreach (BattleEntity entity in barrier)
            {
                if (!param.player.facingEast)
                {
                    entity.rotation = Quaternion.AngleAxis(180, new Vector3(0, 1, 0));
                }
            }
            result.AddRange(barrier);
        }
        if (blinkAction.triggered && !barrierAction.IsPressed())
        {
            var blink = ActivateSkill(3, param.entity);
            foreach (BattleEntity entity in blink)
            {
                entity.facingEast = param.player.facingEast;
            }
            result.AddRange(blink);
        }
        return result;
    }
    public float jumpInitialSpeed = 7.5f;
    public float jumpCurrentSpeed = 0;
    public float jumpGravity = 9.8f;

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * param.timeDiff * definitions.moveSpeed;
        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        if (jumpAction.triggered && param.entity.position.y <= 0 && !barrierAction.IsPressed())
        {
            jumpCurrentSpeed = jumpInitialSpeed;
            moveValue.y = jumpCurrentSpeed * param.timeDiff;
        }
        if (param.entity.position.y > 0 || jumpCurrentSpeed > 0)
        {
            jumpCurrentSpeed -= jumpGravity * param.timeDiff;
            moveValue.y = jumpCurrentSpeed * param.timeDiff;
            if (param.entity.position.y + moveValue.y < 0)
            {
                moveValue.y = -param.entity.position.y;
            }
        }
        else
        {
            moveValue.y = 0;
            jumpCurrentSpeed = 0;
        }
        if (barrierAction.IsPressed())
        {
            // Player cannot move while holding the barrier.
            moveValue.x = 0;
        }
        return moveValue;
    }

    public float godPowerRestore = 0;
    public void Update(EntityUpdateParams param)
    {
        if (param.entity.life <= 0)
        {
            param.entity.isAlive = false;
            return;
        }
        godPowerRestore += param.timeDiff * param.entity.godPowerRecoveryPerSecond;
        if (godPowerRestore > 1)
        {
            int restored = (int)Math.Floor(godPowerRestore);
            godPowerRestore -= restored;
            param.entity.godPower += restored;
            if (param.entity.godPower > param.entity.godPowerMax)
            {
                param.entity.godPower = param.entity.godPowerMax;
            }
        }
    }
}

public class BlinkBehavior : BaseBehavior
{
    TimedProjectionSelfDestructHandler TimedProjectionSelfDestructHandler;

    float blinkDistance;
    public BlinkBehavior(BehaviorDefinitions definitions)
    {
        blinkDistance = definitions.moveSpeed;
        TimedProjectionSelfDestructHandler = new TimedProjectionSelfDestructHandler(definitions.timeToLive);
    }
    bool blinked = false;

    public override AttackDelegate AttackDelegate => SummonEnd;
    bool summoned = false;
    List<BattleEntity> SummonEnd(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (summoned)
        {
            return result;
        }
        summoned = true;
        var entitiesSummoned = param.entity.GetSkillSummon(0, out _);
        foreach (BattleEntity toSummon in entitiesSummoned)
        {
            if (param.entity.facingEast)
            {
                toSummon.position.x += blinkDistance;
            }
            else
            {
                toSummon.position.x -= blinkDistance;
            }
            result.Add(toSummon);
        }
        return result;
    }

    public override SelfDestructDelegate SelfDestructDelegate => Update;
    public void Update(EntityUpdateParams param)
    {
        if (!blinked)
        {
            blinked = true;
            if (param.entity.facingEast)
            {
                param.player.position.x += blinkDistance;
            }
            else
            {
                param.player.position.x -= blinkDistance;
            }
        }
        TimedProjectionSelfDestructHandler.Update(param);
    }
}

public class FireSpreadBehavior
{
    public int spreadRemaining = 3;
    public bool facingEast = true;
    public float spreadDelay = 0.2f;
    public bool spreadHandled = false;

    public List<BattleEntity> Spread(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (spreadRemaining < 0 || spreadHandled)
        {
            return result;
        }
        if (spreadDelay > 0)
        {
            spreadDelay -= param.timeDiff;
            return result;
        }
        var entitiesSummoned = param.entity.GetSkillSummon(0, out _);
        foreach (BattleEntity toSummon in entitiesSummoned)
        {
            if (facingEast)
            {
                toSummon.position.x += 0.8f;
            }
            else
            {
                toSummon.position.x -= 0.8f;
            }
            FireSpreadBehavior behavior = new FireSpreadBehavior();
            behavior.spreadRemaining = spreadRemaining - 1;
            behavior.facingEast = facingEast;
            toSummon.attackHandler = behavior.Spread;
            result.Add(toSummon);
            spreadHandled = true;
        }
        return result;
    }
}

public class LightningControllerBehavior
{
    public float cooldown = 0;

    public List<BattleEntity> MaybeLightning(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (cooldown > 0)
        {
            cooldown -= param.timeDiff;
            return result;
        }
        var entitiesSummoned = param.entity.GetSkillSummon(0, out cooldown);
        foreach (BattleEntity toSummon in entitiesSummoned)
        {
            // Random
            if (Random.Range(0f, 1f) < 0.5)
            {
                break;
            }
            toSummon.position = new Vector2(param.player.position.x + Random.Range(-2f, 20f), 0);
            result.Add(toSummon);
        }
        return result;
    }
}