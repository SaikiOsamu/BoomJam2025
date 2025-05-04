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

class BombBirdBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;

    public float attackCooldown = 0;
    public float birdMoveSpeed = 0.1f;

    public BombBirdBehavior(BehaviorDefinitions definitions)
    {
        birdMoveSpeed = definitions.moveSpeed;
        moveState.orbitBaseRadiusX = 4;
        moveState.orbitBaseRadiusY = 1;
        moveState.orbitEnableFigure8 = true;
    }

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
                    var entitiesSummoned = param.entity.GetSkillSummon(0, out float cooldown);
                    foreach (BattleEntity toSummon in entitiesSummoned)
                    {
                        toSummon.moveHandler = new BombMoveHandler().Move;
                        toSummon.attackHandler = BombAttack;
                        result.Add(toSummon);
                    }
                    attackCooldown = cooldown;
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
        foreach (BattleEntity bombExplosion in param.entity.GetSkillSummon(0, out _))
        {
            bombExplosion.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
            bombExplosion.collideHandler = new AttackCollideHandler(-1, 150).Update;
            result.Add(bombExplosion);
        }

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
                    Vector2 dynamicCenter = param.player.position + new Vector2(0, 3f);

                    // 初始化状态，只需做一次
                    if (moveState.currentMode != "idle")
                    {
                        MovementHandler2D.SwitchMode(moveState, "idle");
                        moveState.orbitBaseRadiusX = 1.2f;
                        moveState.orbitBaseRadiusY = 0.5f;
                        moveState.orbitWobbleAmplitude = 0.05f;
                        moveState.orbitWobbleFrequency = 2f;
                        moveState.orbitShapeCycleTime = 5f;
                        moveState.orbitShapeBlend = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    }

                    Vector2 orbitPoint = MovementHandler2D.OrbitFancy2D(
                        param.entity.position,
                        dynamicCenter, // 🔁 跟随玩家位置
                        moveState,
                        birdMoveSpeed,
                        0.5f
                    );

                    Vector2 smoothed = MovementHandler2D.EaseIntoTrajectory2D(
                        param.entity.position,
                        orbitPoint,
                        moveState,
                        0.5f
                    );

                    moveValue = smoothed - param.entity.position;
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

                    moveState.startTime = 0;
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