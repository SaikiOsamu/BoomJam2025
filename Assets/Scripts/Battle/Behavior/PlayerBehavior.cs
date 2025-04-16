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

class PlayerBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;

    BehaviorDefinitions definitions;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction barrierAction;
    InputAction skill1Action;
    List<float> skillCooldown = new();

    public PlayerBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        moveAction = definitions.moveAction.action;
        jumpAction = definitions.jumpAction.action;
        attackAction = definitions.attackAction.action;
        barrierAction = definitions.barrierAction.action;
        skill1Action = definitions.skill1Action.action;
        for (int i = 0; i < 20; i++)
        {
            skillCooldown.Add(0);
        }
    }

    private List<BattleEntity> ActivateSkill(int skillIndex, BattleEntity entity)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (skillCooldown[skillIndex] > 0)
        {
            return result;
        }
        else
        {
            var entitiesSummoned = entity.GetSkillSummon(skillIndex, out float cooldown);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
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
        if (skill1Action.triggered)
        {
            var flyingSword = ActivateSkill(1, param.entity);
            foreach (BattleEntity entity in flyingSword)
            {
                entity.moveHandler = new FlyingSwordMoveHandler(param.entity).Move;
            }
            result.AddRange(flyingSword);
        }
        if (barrierAction.triggered && onGround)
        {
            var barrier = ActivateSkill(2, param.entity);
            foreach (BattleEntity entity in barrier)
            {
                if (param.player.facingEast)
                {
                    entity.position.x += 0.2f;
                }
                else
                {
                    entity.position.x -= 0.2f;
                    entity.rotation = Quaternion.AngleAxis(180, new Vector3(0, 1, 0));
                }
            }
            result.AddRange(barrier);
        }
        return result;
    }
    public bool onGround = true;
    public float jumpInitialSpeed = 7.5f;
    public float jumpCurrentSpeed = 0;
    public float jumpGravity = 9.8f;

    public Vector2 Move(EntityUpdateParams param)
    {
        if (barrierAction.IsPressed())
        {
            // Player cannot move while holding the barrier.
            return Vector2.zero;
        }
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * param.timeDiff * definitions.moveSpeed;
        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        if (jumpAction.triggered && onGround)
        {
            jumpCurrentSpeed = jumpInitialSpeed;
            onGround = false;
        }
        if (!onGround)
        {
            jumpCurrentSpeed -= jumpGravity * param.timeDiff;
            moveValue.y = jumpCurrentSpeed * param.timeDiff;
            if (param.entity.position.y + moveValue.y < 0)
            {
                onGround = true;
                moveValue.y = -param.entity.position.y;
            }
        }
        else
        {
            moveValue.y = 0;
        }
        return moveValue;
    }
}