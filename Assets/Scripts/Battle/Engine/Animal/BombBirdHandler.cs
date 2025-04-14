using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class BombMoveHandler
{
    float a = 9.8f;
    float v = 0;

    public Vector2 Move(EntityUpdateParams param)
    {
        v += param.timeDiff * a;
        return new Vector2(0, -v * param.timeDiff);
    }
}

class BombBirdHandler
{
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;
    public float birdMoveSpeed = 3;

    public enum State
    {
        BIRD_STATE_IDLE,
        BIRD_STATE_CHASING_ENEMY,
        BIRD_STATE_RETURNING,
    }

    public State birdState = State.BIRD_STATE_IDLE;

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
        switch (birdState)
        {
            case State.BIRD_STATE_IDLE:
            case State.BIRD_STATE_RETURNING:
                break;
            case State.BIRD_STATE_CHASING_ENEMY:
                if (attackCooldown > 0)
                {
                    attackCooldown -= param.timeDiff;
                }
                else if (IsNearEnemy(param.entities, param.entity))
                {
                    BattleEntity bomb = new BattleEntity();
                    bomb.position = param.entity.position * 1;
                    bomb.color = Color.black;
                    bomb.radius = 0.3f;
                    bomb.isEnemy = false;
                    attackCooldown = attackCooldownWhenAttacked;
                    bomb.moveHandler = new BombMoveHandler().Move;
                    bomb.attackHandler = BombAttack;
                    result.Add(bomb);
                    birdState = State.BIRD_STATE_RETURNING;
                }
                break;
        }
        return result;
    }

    private List<BattleEntity> BombAttack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (param.entity.position.y > 0)
        {
            return result;
        }
        BattleEntity bombExplosion = new BattleEntity();
        bombExplosion.position = param.entity.position * 1;
        bombExplosion.color = Color.cyan;
        bombExplosion.radius = 5;
        bombExplosion.isEnemy = param.entity.isEnemy;
        bombExplosion.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
        bombExplosion.collideHandler = new AttackCollideHandler(false, 5000).Update;
        bombExplosion.isProjector = true;
        result.Add(bombExplosion);

        // Self destruct here.
        param.entity.isAlive = false;

        return result;
    }
    public Vector2 Move(EntityUpdateParams param)
    {
        BattleEntity nearestEntity = FindNearestEnemy(param.entities, param.entity.position);
        Vector2 moveValue = Vector2.zero;
        switch (birdState)
        {
            case State.BIRD_STATE_IDLE:
                if (nearestEntity == null)
                {
                    moveValue = (param.player.position + new Vector2(0, 3) - param.entity.position).normalized
                        * param.timeDiff * birdMoveSpeed;
                    break;
                }
                birdState = State.BIRD_STATE_CHASING_ENEMY;
                break;
            case State.BIRD_STATE_CHASING_ENEMY:
                if (nearestEntity == null)
                {
                    birdState = State.BIRD_STATE_RETURNING;
                }
                else
                {
                    moveValue = (nearestEntity.position + new Vector2(0, 3) - param.entity.position).normalized
                        * param.timeDiff * birdMoveSpeed;
                }
                break;
            case State.BIRD_STATE_RETURNING:
                if (Mathf.Abs(param.player.position.x - param.entity.position.x) < 0.2f)
                {
                    birdState = State.BIRD_STATE_IDLE;
                    break;
                }
                moveValue = (param.player.position + new Vector2(0, 3) - param.entity.position).normalized
                    * param.timeDiff * birdMoveSpeed;
                break;
        }

        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        return moveValue;
    }
}