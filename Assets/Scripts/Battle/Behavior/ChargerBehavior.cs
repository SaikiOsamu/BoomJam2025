using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static BattleEntity;

public class ChargerBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => Update;

    private BehaviorDefinitions definitions;
    public ChargerBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }
    public float skillCooldown = 0;
    public float castTime = 0;
    public bool castingSkill = false;
    public float jumpCurrentSpeed = 0;
    public float jumpGravity = 9.8f;

    public enum Decision
    {
        DECISION_UNKNOWN = 0,
        DECISION_FOLLOWING = 1,
        DECISION_STAB = 2,
        DECISION_STAB_BACK = 3,
    }

    public Decision decision = Decision.DECISION_FOLLOWING;

    public Vector2? stabTarget = null;
    bool flipped = false;

    public float playerPosDiff = 0;
    public float dropCurrentSpeed = 0;
    public float gravity = 9.8f;

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
        if (castingSkill)
        {
            return new Vector2(0, dy);
        }
        playerPosDiff += param.timeDiff;

        Vector2 result = Vector2.zero;

        switch (decision)
        {
            case Decision.DECISION_UNKNOWN:
            default:
                break;
            case Decision.DECISION_FOLLOWING:
                if ((param.entity.position - param.player.position - new Vector2(1.5f * Mathf.Sin(playerPosDiff), 0)).magnitude > 0.5)
                {
                    result = param.timeDiff * definitions.moveSpeed * (param.player.position - param.entity.position + new Vector2(1.5f * Mathf.Sin(playerPosDiff), 0)).normalized;
                }
                break;
            case Decision.DECISION_STAB:
                if (stabTarget == null)
                {
                    break;
                }
                if ((param.entity.position - stabTarget.Value).magnitude > 0.1)
                {
                    result = param.timeDiff * definitions.moveSpeed * 1.5f * (stabTarget.Value - param.entity.position).normalized;
                }
                else
                {
                    decision = Decision.DECISION_STAB_BACK;
                    flipped = true;
                }
                break;
            case Decision.DECISION_STAB_BACK:
                if (stabTarget == null)
                {
                    break;
                }
                if ((param.entity.position - param.player.position - new Vector2(1.5f * Mathf.Sin(playerPosDiff), 0)).magnitude > 0.1)
                {
                    result = param.timeDiff * definitions.moveSpeed * 1.5f * (param.player.position - param.entity.position + new Vector2(1.5f * Mathf.Sin(playerPosDiff), 0)).normalized;
                }
                else
                {
                    decision = Decision.DECISION_FOLLOWING;
                    stabTarget = null;
                }
                break;
        }
        result.y = dy;
        return result;
    }

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (skillCooldown > 0)
        {
            skillCooldown -= param.timeDiff;
        }
        if (flipped)
        {
            var entitiesSummoned = param.entity.GetSkillSummon(0, out _);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                toSummon.moveHandler = toSummonParam =>
                {
                    toSummonParam.entity.position = param.entity.position;
                    return Vector2.zero;
                };
                toSummon.selfDestruct = p =>
                {
                    if (decision != Decision.DECISION_STAB_BACK)
                    {
                        p.entity.isAlive = false;
                    }
                };
                stabTarget = param.player.position;
                result.Add(toSummon);
            }
            flipped = false;
        }

        // If already casting, continue.
        if (castingSkill)
        {
            castTime += param.timeDiff;
            if (castTime > param.entity.GetSkillCasttime(0))
            {
                var entitiesSummoned = param.entity.GetSkillSummon(0, out skillCooldown);
                foreach (BattleEntity toSummon in entitiesSummoned)
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
                    result.Add(toSummon);
                }
                castingSkill = false;
                castTime = 0;
            }
            return result;
        }

        // Use the skill if satisify condition and not in cooldown.
        if (skillCooldown <= 0)
        {
            switch (decision)
            {
                case Decision.DECISION_STAB:
                    // Start casting if not started.
                    if (stabTarget != null)
                    {
                        castTime += param.timeDiff;
                        castingSkill = true;
                    }
                    break;
            }
        }


        return result;
    }
    public void Update(EntityUpdateParams param)
    {
        // Make a decision (maybe)
        if (skillCooldown <= 0 && decision == Decision.DECISION_FOLLOWING)
        {
            foreach (BattleEntity entity in param.entities)
            {
                if (!entity.isEnemy)
                {
                    continue;
                }
                decision = Decision.DECISION_STAB;
                stabTarget = param.player.position + new Vector2(20, -param.player.position.y);
                break;
            }

        }
    }
}
