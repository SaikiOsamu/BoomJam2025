using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

public class FlyingSwordMoveHandler
{
    BattleEntity player;
    public float speed = 2;

    public FlyingSwordMoveHandler(BattleEntity player)
    {
        this.player = player;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        param.entity.facingEast = player.facingEast;
        Vector2 moveValue = new Vector2(param.entity.facingEast ? 1 : -1, 0) * param.timeDiff * speed;
        return moveValue;
    }
}

