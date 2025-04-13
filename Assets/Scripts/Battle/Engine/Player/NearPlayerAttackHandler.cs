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
    public BattleEntity player;
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;
    public NearPlayerAttackHandler(BattleEntity player)
    {
        this.player = player;
    }

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else if ((player.position - param.entity.position).magnitude < 40)
        {
            BattleEntity projection = new BattleEntity();
            projection.position = param.entity.position * 1;
            if (player.position.x > param.entity.position.x)
            {
                projection.position.x += 40;
            } else
            {
                projection.position.x -= 40;
            }
            projection.radius = 70;
            projection.isEnemy = true;
            attackCooldown = attackCooldownWhenAttacked;
            projection.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
            projection.collideHandler = new AttackCollideHandler(false).Update;
            projection.isProjector = true;
            result.Add(projection);
        }
        return result;
    }
}