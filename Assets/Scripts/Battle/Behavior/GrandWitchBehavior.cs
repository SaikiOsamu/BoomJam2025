using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

class StubBehavior
{
    public BattleEntity? follow = null;
    public Vector2 followOffset = Vector2.zero;
    public Vector2? target = null;
    public float timeExisted = 0;
    public float? autoFire = null;
    public bool fired = false;
    static public Quaternion GetQuaternion(Vector2 from, Vector2 to)
    {

        float diffx = from.x - to.x;
        float diffy = from.y - to.y;
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
        return Quaternion.AngleAxis(sine / Mathf.PI / 2 * 360, new Vector3(0, 0, 1));
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        if (target == null || autoFire > timeExisted)
        {
            if (follow == null)
            {
                return Vector2.zero;
            }
            Vector2 dir = follow.position - param.entity.position + followOffset;
            param.entity.facingEast = follow.facingEast;
            return dir;
        }
        param.entity.rotation = GetQuaternion(param.entity.position, target.Value);
        return Vector2.zero;
    }

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (target == null || autoFire > timeExisted)
        {
            return result;
        }
        if (fired)
        {
            return result;
        }
        var entitiesSummoned = param.entity.GetSkillSummon(0, out _);

        foreach (BattleEntity toSummon in entitiesSummoned)
        {
            // When this fires, override the move.
            var attackDelegate = toSummon.attackHandler;
            toSummon.rotation = GetQuaternion(toSummon.position, target.Value);
            toSummon.attackHandler = p =>
            {
                var attacked = attackDelegate(p);
                foreach (BattleEntity toAttack in attacked)
                {
                    // Throw it out
                    float speed = toAttack.prefabCharacter?.behavior.moveSpeed ?? 0;
                    if (speed != 0)
                    {
                        toAttack.moveHandler = new VelocityMoveHandler(
                            speed, (target.Value - toSummon.position).normalized).Move;
                    }
                    toAttack.rotation = toSummon.rotation;
                }
                if (attacked.Count > 0)
                {
                    param.entity.isAlive = false;
                }
                return attacked;
            };
            result.Add(toSummon);
        }
        fired = true;
        return result;
    }
}

class BlackSwordBehavior
{
    public BattleEntity witch;
    public float skillCooldown = 0;

    public float castTime = 0;
    public int castingSkill = -1;
    public float speed = 4;

    public enum Decision
    {
        DECISION_UNKNOWN = 0,
        DECISION_FOLLOWING = 1,
        DECISION_ATTACK = 2,
        DECISION_STAB = 3,
    }

    public Decision decision = Decision.DECISION_FOLLOWING;

    public Vector2? stabTarget = null;
    public bool stabForwarding = true;

    public Vector2 GetWitchFollowingPosition()
    {
        if (witch.facingEast)
        {
            return new Vector2(0.3f, 2.5f) + witch.position;
        }
        else
        {
            return new Vector2(-0.3f, 2.5f) + witch.position;
        }
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        if (castingSkill >= 0)
        {
            return Vector2.zero;
        }

        Vector2 result = Vector2.zero;

        switch (decision)
        {
            case Decision.DECISION_UNKNOWN:
            default:
                break;
            case Decision.DECISION_FOLLOWING:
                if ((param.entity.position - GetWitchFollowingPosition()).magnitude > 0.5)
                {
                    result = param.timeDiff * speed * (GetWitchFollowingPosition()-param.entity.position).normalized;
                }
                break;
            case Decision.DECISION_ATTACK:
                if ((param.entity.position - param.player.position).magnitude > 0.5)
                {
                    result = param.timeDiff * speed * (param.player.position - param.entity.position).normalized;
                }
                break;
            case Decision.DECISION_STAB:
                if (stabTarget == null)
                {
                    break;
                }
                else if (stabForwarding)
                {
                    if ((param.entity.position - stabTarget.Value).magnitude > 0.1)
                    {
                        result = param.timeDiff * speed * 1.5f * (stabTarget.Value - param.entity.position).normalized;
                    }
                    else
                    {
                        stabForwarding = false;
                    }
                }
                else
                {
                    if ((param.entity.position - GetWitchFollowingPosition()).magnitude > 0.1)
                    {
                        result = param.timeDiff * speed * 1.5f * (GetWitchFollowingPosition() - param.entity.position).normalized;
                    }
                    else
                    {
                        decision = Decision.DECISION_FOLLOWING;
                        stabTarget = null;
                        stabForwarding = true;
                        skillCooldown = 7;
                    }
                }
                break;
        }
        //if (result.x != 0)
        //{
        //    param.entity.facingEast = result.x > 0;
        //}
        param.entity.rotation = StubBehavior.GetQuaternion(Vector2.zero, result);
        return result;
    }

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (skillCooldown > 0)
        {
            skillCooldown -= param.timeDiff;
        }

        // If already casting, continue.
        if (castingSkill >= 0)
        {
            castTime += param.timeDiff;
            if (castTime > param.entity.GetSkillCasttime(castingSkill))
            {
                var entitiesSummoned = param.entity.GetSkillSummon(castingSkill, out _);
                foreach (BattleEntity toSummon in entitiesSummoned)
                {
                    if (castingSkill == 0)
                    {
                        toSummon.moveHandler = toSummonParam =>
                        {
                            toSummonParam.entity.position = param.entity.position;
                            return Vector2.zero;
                        };
                    }
                    if (castingSkill == 1)
                    {
                        toSummon.moveHandler = toSummonParam =>
                        {
                            toSummonParam.entity.position = param.entity.position;
                            return Vector2.zero;
                        };
                        toSummon.selfDestruct = p =>
                        {
                            if (decision != Decision.DECISION_STAB)
                            {
                                p.entity.isAlive = false;
                            }
                        };
                        stabTarget = param.player.position;
                        stabForwarding = true;
                    }
                    result.Add(toSummon);
                }
                if (castingSkill == 0)
                {
                    decision = Decision.DECISION_FOLLOWING;
                    skillCooldown = 7;
                }
                castingSkill = -1;
                castTime = 0;
            }
            return result;
        }

        // Use the skill if satisify condition and not in cooldown.
        if (skillCooldown <= 0)
        {
            switch (decision)
            {
                case Decision.DECISION_ATTACK:
                    if ((param.player.position - param.entity.position).magnitude < 0.5f)
                    {
                        // Start casting.
                        castTime += param.timeDiff;
                        castingSkill = 0;
                    }
                    break;
                case Decision.DECISION_STAB:
                    // Start casting if not started.
                    if (stabTarget == null)
                    {
                        castTime += param.timeDiff;
                        castingSkill = 1;
                    }
                    break;
            }
        }


        return result;
    }
    LifeBasedSelfDestructHandler selfDestructHandler = new();
    public void Update(EntityUpdateParams param)
    {
        selfDestructHandler.Update(param);
        if (!param.entity.isAlive)
        {
            return;
        }
        // Make a decision (maybe)
        if (skillCooldown <= 0 && decision == Decision.DECISION_FOLLOWING)
        {
            float d = Random.Range(0f, 1f);
            if (d < 0.6f)
            {
                decision = Decision.DECISION_ATTACK;

            }
            else
            {
                decision = Decision.DECISION_STAB;
            }
        }
    }
}

class GrandWitchBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    BehaviorDefinitions definitions;

    public GrandWitchBehavior(BehaviorDefinitions definitions)
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
        if (castingSkill >= 0)
        {
            return new Vector2(0, dy);
        }
        if ((param.player.position - param.entity.position).magnitude < meleeDistance)
        {
            return new Vector2(0, dy);
        }
        Vector2 moveValue = (param.player.position - param.entity.position).normalized
            * param.timeDiff
            * definitions.moveSpeed;
        param.entity.facingEast = moveValue.x > 0;
        moveValue.y = dy;
        return moveValue;
    }

    public float castTime = 0;
    public int castingSkill = -1;
    public float meleeDistance = 2.2f;

    public float[] skillCooldowns = new float[4];

    public StubBehavior[] stubStored = new StubBehavior[3];

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        //if ((float)param.entity.life / param.entity.lifeMax < 0.2)
        //{
        //    isBloodlust = true;
        //}

        for (int i = 0; i < 4; ++i)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= param.timeDiff;
            }
        }

        // Maybe fire the ice stubs to player.
        for (int i = 0; i < 3; ++i)
        {
            if (stubStored[i]?.timeExisted > 5)
            {
                stubStored[i].target = param.player.position;
                stubStored[i] = null;
            }
        }

        // If already casting, continue.
        if (castingSkill >= 0)
        {
            castTime += param.timeDiff;
            if (castTime > param.entity.GetSkillCasttime(castingSkill))
            {
                var entitiesSummoned = param.entity.GetSkillSummon(castingSkill, out float cooldown);
                foreach (BattleEntity toSummon in entitiesSummoned)
                {
                    if (castingSkill == 1)
                    {
                        StubBehavior behavior = null;
                        for (int i = 0; i < 3; ++i)
                        {
                            if (stubStored[i] != null)
                            {
                                continue;
                            }
                            behavior = new StubBehavior();
                            behavior.follow = param.entity;
                            switch (i)
                            {
                                case 0:
                                    behavior.followOffset = new Vector2(1.7f, 1.7f);
                                    break;
                                case 1:
                                    behavior.timeExisted -= 0.5f;
                                    behavior.followOffset = new Vector2(0, 2.3f);
                                    break;
                                case 2:
                                    behavior.timeExisted -= 1f;
                                    behavior.followOffset = new Vector2(-1.7f, 1.7f);
                                    break;
                            }
                            stubStored[i] = behavior;
                            break;
                        }
                        if (behavior == null)
                        {
                            break;
                        }
                        toSummon.moveHandler = behavior.Move;
                        toSummon.attackHandler = behavior.Attack;
                        toSummon.selfDestruct = param => behavior.timeExisted += param.timeDiff;
                    }
                    if (castingSkill >= 2 && castingSkill <= 6)
                    {
                        StubBehavior behavior = new StubBehavior();
                        behavior.autoFire = 2;
                        // Randomly spawn on edge.
                        int side = Random.Range(0, 3);
                        float position = Random.Range(0f, 1f);
                        switch (side)
                        {
                            case 0:
                                {
                                    Vector2 pos = new Vector2(
                                        param.bossFightCenter.Value.x + param.bossFightSize.Value,
                                        10 * position);
                                    toSummon.position = pos;
                                    toSummon.rotation = StubBehavior.GetQuaternion(Vector2.zero, new Vector2(-1, 0));
                                    behavior.target = pos + new Vector2(-1, 0);
                                }
                                break;
                            case 1:
                                {
                                    Vector2 pos = new Vector2(
                                        param.bossFightCenter.Value.x - param.bossFightSize.Value,
                                        10 * position);
                                    toSummon.position = pos;
                                    toSummon.rotation = StubBehavior.GetQuaternion(Vector2.zero, new Vector2(1, 0));
                                    behavior.target = pos + new Vector2(1, 0);
                                }
                                break;
                            case 2:
                                {
                                    Vector2 pos = new Vector2(
                                        param.bossFightCenter.Value.x + param.bossFightSize.Value * (position * 2 - 1),
                                        10);
                                    toSummon.position = pos;
                                    toSummon.rotation = StubBehavior.GetQuaternion(Vector2.zero, new Vector2(0, -1));
                                    behavior.target = pos + new Vector2(0, -1);
                                }
                                break;
                        }
                        toSummon.moveHandler = behavior.Move;
                        toSummon.attackHandler = behavior.Attack;
                        toSummon.selfDestruct = param => behavior.timeExisted += param.timeDiff;

                    }
                    if (castingSkill == 7)
                    {
                        BlackSwordBehavior behavior = new BlackSwordBehavior();
                        behavior.witch = param.entity;
                        toSummon.position = param.entity.position;
                        toSummon.moveHandler = behavior.Move;
                        toSummon.attackHandler = behavior.Attack;
                        toSummon.selfDestruct = behavior.Update;
                    }
                    result.Add(toSummon);
                }
                switch (castingSkill)
                {
                    case 0:
                        skillCooldowns[0] = cooldown;
                        castingSkill = -1;
                        break;
                    case 1:
                        skillCooldowns[1] = cooldown;
                        castingSkill = -1;
                        break;
                    case 2:
                        castingSkill = 3;
                        break;
                    case 3:
                        castingSkill = 4;
                        break;
                    case 4:
                        castingSkill = 5;
                        break;
                    case 5:
                        castingSkill = 6;
                        break;
                    case 6:
                        skillCooldowns[2] = cooldown;
                        castingSkill = -1;
                        break;
                    case 7:
                        skillCooldowns[3] = cooldown;
                        castingSkill = -1;
                        break;
                }
                castTime = 0;
            }
            return result;
        }

        // Use the skill if satisify condition and not in cooldown.
        if (skillCooldowns[1] <= 0)
        {
            // Start casting.
            castTime += param.timeDiff;
            castingSkill = 1;
            return result;
        }

        if (skillCooldowns[2] <= 0 && ((float)param.entity.life / param.entity.lifeMax < 0.8))
        {
            // Start casting.
            castTime += param.timeDiff;
            castingSkill = 2;
            return result;
        }

        if (skillCooldowns[3] <= 0 && ((float)param.entity.life / param.entity.lifeMax < 0.6))
        {
            // Start casting.
            castTime += param.timeDiff;
            castingSkill = 7;
            return result;
        }

        if ((param.player.position - param.entity.position).magnitude < meleeDistance && skillCooldowns[0] <= 0)
        {
            var entitiesSummoned = param.entity.GetSkillSummon(0, out float cooldown);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                if (param.entity.facingEast)
                {
                    toSummon.position.x += 1.5f;
                }
                else
                {
                    toSummon.position.x -= 1.5f;
                }
                result.Add(toSummon);
            }
            skillCooldowns[0] = cooldown;

            // Maybe fire the ice stubs to player.
            for (int i = 0; i < 3; ++i)
            {
                if (stubStored[i]?.timeExisted > 5)
                {
                    stubStored[i].target = param.player.position;
                    stubStored[i] = null;
                }
            }
        }
        return result;
    }
}