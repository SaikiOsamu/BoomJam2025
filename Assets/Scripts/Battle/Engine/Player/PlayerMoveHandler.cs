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
    InputAction jumpAction;
    public float speed = 1;
    public bool onGround = true;
    public float jumpInitialSpeed = 7.5f;
    public float jumpCurrentSpeed = 0;
    public float jumpGravity = 9.8f;

    public PlayerMoveHandler(InputAction moveAction, InputAction jumpAction)
    {
        this.moveAction = moveAction;
        this.jumpAction = jumpAction;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * param.timeDiff * speed;
        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        if (jumpAction.triggered && onGround)
        {
            jumpCurrentSpeed = jumpInitialSpeed;
            onGround = false;
        }
        if (!onGround)
        {
            jumpCurrentSpeed -= jumpGravity * param.timeDiff;
            moveValue.y = jumpCurrentSpeed * param.timeDiff;
            if (param.entity.position.y + moveValue.y < 0)
            {
                onGround = true;
                moveValue.y = -param.entity.position.y;
            }
        }
        else
        {
            moveValue.y = 0;
        }
        return moveValue;
    }
}

