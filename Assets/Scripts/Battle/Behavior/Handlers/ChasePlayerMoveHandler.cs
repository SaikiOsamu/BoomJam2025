using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

public class ChasePlayerMoveHandler
{
    public float speed = 0.5f;

    public ChasePlayerMoveHandler(float speed)
    {
        this.speed = speed;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = (param.player.position - param.entity.position).normalized * param.timeDiff * speed;
        param.entity.facingEast = moveValue.x > 0;
        return moveValue;
    }
}
