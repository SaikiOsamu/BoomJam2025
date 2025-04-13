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
    InputAction skillAction;
    Sprite flyingSwordSprite;
    public PlayerAttackHandler(InputAction attackAction, InputAction skillAction)
    {
        this.attackAction = attackAction;
        this.skillAction = skillAction;
        flyingSwordSprite = Resources.Load<Sprite>("56321ef667a8d4ecc1c19230419ef7aa");
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
            }
            else
            {
                projection.position.x -= 40;
            }
            projection.color = Color.yellow;
            projection.radius = 70;
            projection.isEnemy = false;
            attackCooldown = attackCooldownWhenAttacked;
            projection.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
            projection.collideHandler = new AttackCollideHandler(false, 5000).Update;
            projection.isProjector = true;
            result.Add(projection);
        }
        if (skillAction.triggered)
        {
            BattleEntity flyingSword = new BattleEntity();
            flyingSword.position = param.entity.position * 1;
            if (param.entity.facingEast)
            {
                flyingSword.position.x += 40;
            }
            else
            {
                flyingSword.position.x -= 40;
            }
            flyingSword.sprite = flyingSwordSprite;
            flyingSword.radius = 70;
            flyingSword.isEnemy = false;
            flyingSword.moveHandler = new FlyingSwordMoveHandler(param.entity).Move;
            flyingSword.selfDestruct = new TimedProjectionSelfDestructHandler(15.0f).Update;
            flyingSword.collideHandler = new AttackCollideHandler(false, 5000).Update;
            flyingSword.isProjector = true;
            result.Add(flyingSword);
        }
        return result;
    }
}