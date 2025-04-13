using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class PlayerAttackHandler
{
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;

    InputAction attackAction;
    public PlayerAttackHandler(InputAction attackAction)
    {
        this.attackAction = attackAction;
    }

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else if (attackAction.triggered)
        {
            BattleEntity projection = new BattleEntity();
            projection.position = param.entity.position * 1;
            if (param.entity.facingEast)
            {
                projection.position.x += 40;
            } else
            {
                projection.position.x -= 40;
            }
            projection.radius = 70;
            projection.isEnemy = false;
            attackCooldown = attackCooldownWhenAttacked;
            projection.selfDestruct = new TimedProjectionSelfDestructHandler(1.0f).Update;
            projection.collideHandler = new AttackCollideHandler(false, 5000).Update;
            result.Add(projection);
        }
        return result;
    }
}