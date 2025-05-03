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

class FloatingCannonBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;

    public float attackCooldown = 0;
    public float birdMoveSpeed = 3;

    public FloatingCannonBehavior(BehaviorDefinitions definitions)
    {
        birdMoveSpeed = definitions.moveSpeed;
    }

    bool isAttacking = false;

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

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else
        {
            if (isAttacking)
            {
                return result;
            }
            var enemy = FindNearestEnemy(param.entities, param.entity.position);
            if (enemy != null)
            {
                var entitiesSummoned = param.entity.GetSkillSummon(0, out float cooldown);
                foreach (BattleEntity toSummon in entitiesSummoned)
                {
                    float diffx = toSummon.position.x - enemy.position.x;
                    float diffy = toSummon.position.y - enemy.position.y;
                    float sine;
                    if (diffx != 0)
                    {
                        sine = Mathf.Asin(diffy / diffx);
                    }
                    else
                    {
                        sine = diffy > 0 ? -Mathf.PI / 2 : Mathf.PI / 2;
                    }
                    if (diffx > 0)
                    {
                        sine += Mathf.PI;
                    }
                    toSummon.rotation = Quaternion.AngleAxis(sine / Mathf.PI / 2 * 360, new Vector3(0, 0, 1));
                    toSummon.position = param.entity.position * 1;
                    toSummon.collideHandler = LazerAttack;
                    toSummon.selfDestruct = LazerUpdate;
                    lazerTime = 0;
                    collidedObjects.Clear();
                    result.Add(toSummon);
                }
                attackCooldown = cooldown;
                isAttacking = true;
            }
        }
        return result;
    }

    float lazerTime = 0;
    public int attack = 5;
    public HashSet<BattleEntity> collidedObjects = new HashSet<BattleEntity>(ReferenceEqualityComparer.Instance);

    private void LazerUpdate(BattleEntity.EntityUpdateParams param)
    {
        lazerTime += param.timeDiff;
        if (lazerTime > 3.5)
        {
            param.entity.isAlive = false;
            isAttacking = false;
        }
    }

    private bool LazerAttack(BattleEntity.EntityUpdateParams param, BattleEntity theOtherEntity)
    {
        if (lazerTime < 2.15)
        {
            return false;
        }
        if (lazerTime < 2.85)
        {
            if (collidedObjects.Contains(theOtherEntity))
            {
                return false;
            }
            if (param.entity.isEnemy == theOtherEntity.isEnemy)
            {
                return false;
            }
            theOtherEntity.Damage(attack);
            collidedObjects.Add(theOtherEntity);
            return true;
        }
        return false;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = (param.player.position + new Vector2(0, 1) - param.entity.position).normalized
            * param.timeDiff * birdMoveSpeed;

        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        return moveValue;
    }
}