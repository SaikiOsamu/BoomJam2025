using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class NaofuHandler
{
    public enum State
    {
        STATE_IDLE = 0,
        STATE_CHASING_ENEMY = 1,
        STATE_RETURNING = 2,
        STATE_ATTACKING = 3
    }
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 5;
    public float moveSpeed = 1;
    public float attackingSpeed = 3;
    public State state = State.STATE_IDLE;
    private bool moveRight = true;
    private bool isAttacking = false;
    private BattleEntity target;
    private bool isPouncing = false;
    private float pounceTimer = 0f;
    private Vector2 pounceTarget;
    float randomFactor = UnityEngine.Random.Range(0.9f, 1.1f);



    public bool IsNearEnemy(ReadOnlyCollection<BattleEntity> entities, BattleEntity entity)
    {
        foreach (var e in entities)
        {
            if (e.isEnemy == entity.isEnemy)
            {
                continue;
            }
            //Debug.Log(Mathf.Abs(e.position.x - entity.position.x) );
            if (Mathf.Abs(e.position.x - entity.position.x) < 0.5)
            {
                return true;
            }
        }
        return false;
    }

    public BattleEntity returnTarget(ReadOnlyCollection<BattleEntity> entities, BattleEntity entity)
    {
        foreach (var e in entities)
        {
            if (e.isEnemy == entity.isEnemy)
            {
                continue;
            }
            if (Mathf.Abs(e.position.x - entity.position.x) < 0.5)
            {
                return e;
            }
        }
        return null;
    }
    
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

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        switch (state)
        {
            case State.STATE_IDLE:
            case State.STATE_RETURNING:
                break;
            case State.STATE_CHASING_ENEMY:
                // if (attackCooldown > 0){
                //     attackCooldown -= param.timeDiff;
                // }
                // else if (IsNearEnemy(param.entities, param.entity)){
                //     target = returnTarget(param.entities, param.entity);
                //     isAttacking = true;
                //     state = State.STATE_ATTACKING;
                // }
                break;
            case State.STATE_ATTACKING:
                // if (target!=null && target.isAlive) {
                //
                // }else{
                //     state = State.STATE_IDLE;
                //     isAttacking = false;
                //     target = null;
                // }
                break;

        }
        return result;
    }

    public Vector2 Move(BattleEntity.EntityUpdateParams param)
    {
        BattleEntity nearestEntity = FindNearestEnemy(param.entities, param.entity.position);
        Vector2 moveValue = Vector2.zero;
        moveSpeed = attackCooldown > 0 ? 0.2f : 1;
        if (attackCooldown > 0){
            attackCooldown -= param.timeDiff;
        }
        switch (state)
        {
            case State.STATE_IDLE:
                if (nearestEntity == null)
                {
                    float distanceToPlayer = param.entity.position.x - param.player.position.x;
                    if (distanceToPlayer > 2f)
                    {
                        moveRight = false;
                    }
                    else if (distanceToPlayer < -2f)
                    {
                        moveRight = true;
                    }

                    moveValue.x = moveRight ? 1f : -1f;
                    moveValue *= param.timeDiff * moveSpeed * randomFactor;
                    break;
                }
                state = State.STATE_CHASING_ENEMY;
                break;
            case State.STATE_CHASING_ENEMY:
                if (nearestEntity == null)
                {
                    state = State.STATE_IDLE;
                }
                else
                {
                    moveValue = (nearestEntity.position - param.entity.position).normalized;
                    moveValue *= param.timeDiff * moveSpeed;
                }

                if (IsNearEnemy(param.entities, param.entity) && attackCooldown<=0)
                {
                    target = returnTarget(param.entities, param.entity);
                    attackCooldown = 3;
                    state = State.STATE_ATTACKING;
                }
                break;
            case State.STATE_ATTACKING:
            {
                if (target == null)
                {
                    state = State.STATE_IDLE;
                }
                else if (attackCooldown>2.5f)
                {
                    float offset = param.entity.position.x < nearestEntity.position.x ? 1f : -1f;
                    Vector2 pounceTarget = new Vector2(nearestEntity.position.x + offset, nearestEntity.position.y);
                    moveValue = pounceTarget - param.entity.position;
                    state = State.STATE_CHASING_ENEMY;

                }
                else
                {
                    state = State.STATE_CHASING_ENEMY;
                }

                break;
            }

        }

        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        return moveValue;
    }
}