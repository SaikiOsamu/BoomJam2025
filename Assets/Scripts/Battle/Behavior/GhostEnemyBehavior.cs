﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class GhostEnemyBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    BehaviorDefinitions definitions;

    public GhostEnemyBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }

    public float dropCurrentSpeed = 0;
    public float gravity = 9.8f;
    public bool onGround = true;
    public bool isHidden = false;
    public float hiddenCooldown = 0;

    public Vector2 Move(EntityUpdateParams param)
    {
        // Handle drop
        float dy = 0;
        if (param.entity.position.y > 0)
        {
            dropCurrentSpeed -= gravity * param.timeDiff;
            dy = dropCurrentSpeed * param.timeDiff;
            if (param.entity.position.y + dy < 0)
            {
                dy = -param.entity.position.y;
            }
        }
        else
        {
            dropCurrentSpeed = 0;
        }
        if (castTime > 0)
        {
            return new Vector2(0, dy);
        }
        if ((param.player.position - param.entity.position).magnitude < definitions.attackDistance)
        {
            if (attackCooldown <= 0 && !param.entity.isHidden)
            {
                param.entity.isHidden = true;
            }
            if ((param.player.position - param.entity.position).magnitude < meleeDistance)
            {
                return new Vector2(0, dy);
            }
        }
        Vector2 moveValue = (param.player.position - param.entity.position).normalized * param.timeDiff * definitions.moveSpeed;
        param.entity.facingEast = moveValue.x > 0;
        moveValue.y = dy;
        return moveValue;
    }

    public float attackCooldown = 0;
    public float meleeAttackCooldown = 0;
    public float castTime = 0;
    public float meleeDistance = 0.4f;

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (meleeAttackCooldown > 0)
        {
            meleeAttackCooldown -= param.timeDiff;
        }
        float skillCastTimeRequired = param.entity.GetSkillCasttime(0);
        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
            if (meleeAttackCooldown <= 0
                && (param.player.position - param.entity.position).magnitude < meleeDistance
                && (param.entity.prefabCharacter?.skills.Count ?? 0) > 1)
            {
                // Use the last skill, which is the attack.
                var entitiesSummoned = param.entity.GetSkillSummon(1, out float cooldown);
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
                    result.Add(toSummon);
                }
                meleeAttackCooldown = cooldown;
            }
        }
        else if (param.entity.isHidden && (param.player.position - param.entity.position).magnitude < meleeDistance)
        {
            if (castTime > skillCastTimeRequired)
            {
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
                            new Vector2(0, 1)).Move;
                    }
                    result.Add(toSummon);
                }
                attackCooldown = cooldown;
                castTime = 0;
                param.entity.isHidden = false;
            }
            else
            {
                castTime += param.timeDiff;
            }
        }
        else if (castTime > 0 && castTime < skillCastTimeRequired)
        {
            castTime += param.timeDiff;
        }
        else if (castTime > skillCastTimeRequired)
        {
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
                        new Vector2(0, 1)).Move;
                }
                result.Add(toSummon);
            }
            attackCooldown = cooldown;
            castTime = 0;
            param.entity.isHidden = false;
        }
        return result;
    }
}