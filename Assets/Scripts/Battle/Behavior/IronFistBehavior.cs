using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;
using Random = UnityEngine.Random;

class IronFistBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => Move;
    public override AttackDelegate AttackDelegate => Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    BehaviorDefinitions definitions;

    public IronFistBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        skillCooldowns[3] = 7;
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
        // Iron fist move
        if (duringHeavyFist)
        {
            return new Vector2(param.timeDiff
                * definitions.moveSpeed
                * (isBloodlust ? 2 : 1.5f)
                * (param.entity.facingEast ? 1 : -1), dy);
        }
        if (castingSkill >= 0)
        {
            return new Vector2(0, dy);
        }
        if ((param.player.position - param.entity.position).magnitude < heavyFistDistance)
        {
            return new Vector2(0, dy);
        }
        Vector2 moveValue = (param.player.position - param.entity.position).normalized
            * param.timeDiff
            * definitions.moveSpeed
            * (isBloodlust ? 1.5f : 1);
        param.entity.facingEast = moveValue.x > 0;
        moveValue.y = dy;
        return moveValue;
    }

    public float castTime = 0;
    public int castingSkill = -1;
    public float heavyFistDistance = 2.2f;
    public float multipleHeavyFistDistance = 2.2f;

    public float[] skillCooldowns = new float[4];
    public bool isBloodlust = false;
    public bool duringHeavyFist = false;
    public int remainingShockwaveCount = -1;
    public float behaviorRandom = 0;
    public float behaviorDecisionCooldown = 0;

    private int? ChooseSkillToUse()
    {
        // Decision is made every 0.5 seconds.
        if (behaviorDecisionCooldown > 0.5)
        {
            behaviorDecisionCooldown = 0;
            behaviorRandom = Random.Range(0f, 1f);
        }
        bool anySkill = false;
        float skill1Needed = isBloodlust ? 0 : 0.5f;
        float skill2Needed = isBloodlust ? 0.5f : 0.25f;
        float skill3Needed = isBloodlust ? 0.5f : 0.25f;
        if (skillCooldowns[0] > 0)
        {
            skill1Needed = 0;
        }
        else
        {
            anySkill = !isBloodlust;
        }
        if (skillCooldowns[1] > 0)
        {
            skill2Needed = 0;
        }
        else
        {
            anySkill = true;
        }
        if (skillCooldowns[2] > 0)
        {
            skill3Needed = 0;
        }
        else
        {
            anySkill = true;
        }
        if (!anySkill)
        {
            return null;
        }

        float behaviorRandomToUse = behaviorRandom * (skill1Needed + skill2Needed + skill3Needed);
        if (behaviorRandomToUse < skill1Needed)
        {
            return 0;
        }
        behaviorRandomToUse -= skill1Needed;
        if (behaviorRandomToUse < skill2Needed)
        {
            return 1;
        }
        behaviorRandomToUse -= skill2Needed;
        if (behaviorRandomToUse < skill3Needed)
        {
            return 6;
        }
        // This should not happen.. Let's do nothing instead.
        return null;
    }

    private float GetSkillDistance(int skill)
    {
        switch (skill)
        {
            case 0:
                return heavyFistDistance;
            case 1:
                return multipleHeavyFistDistance;
            case 6:
                return 15;
            default:
                return 0;
        }
    }

    public List<BattleEntity> Attack(EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if ((float)param.entity.life / param.entity.lifeMax < 0.2)
        {
            isBloodlust = true;
        }

        for (int i = 0; i < 4; ++i)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= param.timeDiff;
            }
        }
        behaviorDecisionCooldown += param.timeDiff;

        // If already casting, continue.
        if (castingSkill >= 0)
        {
            castTime += param.timeDiff;
            if (castTime > param.entity.GetSkillCasttime(castingSkill))
            {
                var entitiesSummoned = param.entity.GetSkillSummon(castingSkill, out float cooldown);
                bool shockwaveHandled = false;
                foreach (BattleEntity toSummon in entitiesSummoned)
                {
                    // Fists shall stick to self.
                    if (castingSkill < 6)
                    {
                        toSummon.moveHandler = toSummonParam =>
                        {
                            if (param.entity.facingEast)
                            {
                                toSummonParam.entity.position.x = param.entity.position.x + 1.4f;
                            }
                            else
                            {
                                toSummonParam.entity.position.x = param.entity.position.x - 1.4f;
                            }
                            return Vector2.zero;
                        };
                        var originalDestruct = toSummon.selfDestruct;
                        toSummon.selfDestruct = p =>
                        {
                            originalDestruct.Invoke(p);
                            if (!p.entity.isAlive)
                            {
                                duringHeavyFist = false;
                            }
                        };
                        duringHeavyFist = true;
                    }
                    if (castingSkill == 6)
                    {
                        float speed = toSummon.prefabCharacter?.behavior.moveSpeed ?? 0;
                        toSummon.moveHandler = new VelocityMoveHandler(
                                speed,
                                new Vector2(shockwaveHandled ? -1 : 1, 0)).Move;
                        shockwaveHandled = true;
                    }
                    result.Add(toSummon);
                }
                switch (castingSkill)
                {
                    case 0:
                        skillCooldowns[0] = cooldown;
                        skillCooldowns[3] = 7;
                        castingSkill = -1;
                        break;
                    case 1:
                        castingSkill = 2;
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
                        skillCooldowns[1] = cooldown;
                        skillCooldowns[3] = 7;
                        castingSkill = -1;
                        break;
                    case 6:
                        if (remainingShockwaveCount > 0)
                        {
                            castingSkill = 6;
                            remainingShockwaveCount -= 1;
                        }
                        else
                        {
                            skillCooldowns[2] = cooldown;
                            skillCooldowns[3] = 7;
                            castingSkill = -1;
                            remainingShockwaveCount = -1;
                        }
                        break;
                    case 7:
                        skillCooldowns[3] = 7;
                        castingSkill = -1;
                        break;
                }
                castTime = 0;
            }
            return result;
        }

        if (skillCooldowns[3] <= 0)
        {
            var entitiesSummoned = param.entity.GetSkillSummon(7, out float cooldown);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                result.Add(toSummon);
            }
            skillCooldowns[3] = cooldown;
            return result;
        }

        int? skillToUse = ChooseSkillToUse();


        if ((param.player.position - param.entity.position).magnitude < GetSkillDistance(skillToUse.Value))
        {
            // Start casting.
            castTime += param.timeDiff;
            castingSkill = skillToUse.Value;
            if (skillToUse == 6)
            {
                castTime -= 1.5f;
                remainingShockwaveCount = 2;
            }
        }
        return result;
    }
}