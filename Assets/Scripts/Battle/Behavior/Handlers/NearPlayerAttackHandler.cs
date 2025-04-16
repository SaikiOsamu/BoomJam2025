using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class NearPlayerAttackHandler
{
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else if ((param.player.position - param.entity.position).magnitude < 0.4f)
        {
            BattleEntity projection = new BattleEntity();
            projection.position = param.entity.position * 1;
            if (param.player.position.x > param.entity.position.x)
            {
                projection.position.x += 0.4f;
            }
            else
            {
                projection.position.x -= 0.4f;
            }
            projection.radius = 0.7f;
            projection.isEnemy = true;
            attackCooldown = attackCooldownWhenAttacked;
            projection.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
            projection.collideHandler = new AttackCollideHandler(-1).Update;
            projection.isProjector = true;
            result.Add(projection);
        }
        return result;
    }
}