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
    public BattleEntity player;
    public float speed = 0.5f;

    public ChasePlayerMoveHandler(BattleEntity player, float speed)
    {
        this.speed = speed;
        this.player = player;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = (player.position - param.entity.position).normalized * param.timeDiff * speed;
        param.entity.facingEast = moveValue.x > 0;
        return moveValue;
    }
}

