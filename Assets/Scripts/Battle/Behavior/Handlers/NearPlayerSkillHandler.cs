using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class NearPlayerSkillHandler
{
    public float attackCooldown = 0;

    public float attackDistance = 0.4f;

    public NearPlayerSkillHandler(float attackDistance)
    {
        this.attackDistance = attackDistance;
    }

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else if ((param.player.position - param.entity.position).magnitude < attackDistance)
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
                        (param.player.position - toSummon.position).normalized).Move;
                }
                result.Add(toSummon);
            }
            attackCooldown = cooldown;
        }
        return result;
    }
}