using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class CobraBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;

    public float attackCooldown = 0;
    public float castTime = 0;
    BehaviorDefinitions definitions;

    public CobraBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }

    public enum State
    {
        COBRA_STATE_IDLE,
        COBRA_STATE_CHASING_ENEMY,
        COBRA_STATE_RETURNING,
    }

    public State birdState = State.COBRA_STATE_IDLE;

    BattleEntity FindNearestEnemy(ReadOnlyCollection<BattleEntity> entities, Vector2 relativeTo)
    {
        BattleEntity battleEntity = null;
        foreach (BattleEntity entity in entities)
        {
            if (!entity.isEnemy)
            {
                continue;
            }
            if (battleEntity == null)
            {
                battleEntity = entity;
                continue;
            }
            if ((battleEntity.position - relativeTo).magnitude > (entity.position - relativeTo).magnitude)
            {
                battleEntity = entity;
            }
        }
        return battleEntity;
    }

    public bool IsNearEnemy(ReadOnlyCollection<BattleEntity> entities, BattleEntity entity)
    {
        foreach (var e in entities)
        {
            if (e.isEnemy == entity.isEnemy)
            {
                continue;
            }
            if (Mathf.Abs(e.position.x - entity.position.x) < 0.5)
            {
                return true;
            }
        }
        return false;
    }

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        BattleEntity nearestEntity = FindNearestEnemy(param.entities, param.entity.position);
        if (attackCooldown > 0)
        {
            param.entity.isAttacking = false;
            attackCooldown -= param.timeDiff;
        }
        else if (nearestEntity != null)
        {
            param.entity.isAttacking = true;
            if (castTime < param.entity.prefabCharacter?.skills.FirstOrDefault()?.castSecond)
            {
                castTime += param.timeDiff;
            }
            var entitiesSummoned = param.entity.GetSkillSummon(0, out float cooldown);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                if (param.entity.facingEast)
                {
                    toSummon.position.x += 0.4f;
                }
                else
                {
                    toSummon.position.x -= 0.4f;
                }
                // Maybe throw it out
                if (toSummon.prefabCharacter != null && toSummon.prefabCharacter.behavior.moveSpeed != 0)
                {
                    toSummon.moveHandler = new VelocityMoveHandler(
                        toSummon.prefabCharacter.behavior.moveSpeed,
                        (nearestEntity.position - toSummon.position).normalized).Move;
                }
                result.Add(toSummon);
            }
            attackCooldown = cooldown;
        }
        else
        {
            castTime = 0;
        }
        return result;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        BattleEntity nearestEntity = FindNearestEnemy(param.entities, param.entity.position);
        Vector2 moveValue = Vector2.zero;
        if (castTime > 0)
        {
            return moveValue;
        }
        if (nearestEntity != null)
        {
            if ((nearestEntity.position - param.entity.position).magnitude > definitions.attackDistance)
            {
                moveValue = (nearestEntity.position - param.entity.position).normalized
                    * param.timeDiff * definitions.moveSpeed;
            }
        }
        else
        {
            moveValue = (param.player.position - param.entity.position).normalized
                * param.timeDiff * definitions.moveSpeed;
        }

        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        return moveValue;
    }
}