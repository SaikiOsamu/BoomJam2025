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
    public float dropCurrentSpeed = 0;
    public float gravity = 9.8f;
    public bool onGround = true;

    public ChasePlayerMoveHandler(float speed, float until = 0)
    {
        this.speed = speed;
        this.until = until;
    }

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
        if ((param.player.position - param.entity.position).magnitude < until)
        {
            return new Vector2(0, dy);
        }
        Vector2 moveValue = (param.player.position - param.entity.position).normalized * param.timeDiff * speed;
        param.entity.facingEast = moveValue.x > 0;
        moveValue.y = dy;
        return moveValue;
    }
}
