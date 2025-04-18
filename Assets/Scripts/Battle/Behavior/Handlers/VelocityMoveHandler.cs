using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

public class VelocityMoveHandler
{
    public float speed = 0.5f;
    public Vector2 direction = Vector2.zero;

    public VelocityMoveHandler(float speed, Vector2 direction)
    {
        this.speed = speed;
        this.direction = direction.normalized;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = direction * param.timeDiff * speed;
        param.entity.facingEast = moveValue.x > 0;
        return moveValue;
    }
}
