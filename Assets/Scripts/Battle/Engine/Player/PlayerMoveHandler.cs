using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

public class PlayerMoveHandler
{
    InputAction moveAction;
    public float speed = 100;

    public PlayerMoveHandler(InputAction moveAction)
    {
        this.moveAction = moveAction;
    }

    public void Move(EntityUpdateParams param)
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * param.timeDiff * speed;
        moveValue.y = 0;
        param.entity.position += moveValue;
        param.entity.facingEast = moveValue.x > 0;
    }
}

