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
    public float until = 0;

    public ChasePlayerMoveHandler(float speed, float until = 0)
    {
        this.speed = speed;
        this.until = until;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        if ((param.player.position - param.entity.position).magnitude < until)
        {
            return Vector2.zero;
        }
        Vector2 moveValue = (param.player.position - param.entity.position).normalized * param.timeDiff * speed;
        param.entity.facingEast = moveValue.x > 0;
        return moveValue;
    }
}
