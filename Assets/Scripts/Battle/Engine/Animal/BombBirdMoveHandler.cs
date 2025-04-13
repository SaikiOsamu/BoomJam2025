using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

public class BombBirdMoveHandler
{
    public float speed = 300;

    public BombBirdMoveHandler()
    {
    }

    BattleEntity FindNearestEnemy(ReadOnlyCollection<BattleEntity> entities, Vector2 relativeTo)
    {
        BattleEntity battleEntity = null;
        foreach (BattleEntity entity in entities)
        {
            if (!entity.isEnemy)
            {
                continue;
            }
            if (battleEntity == null)
            {
                battleEntity = entity;
                continue;
            }
            if ((battleEntity.position - relativeTo).magnitude > (entity.position - relativeTo).magnitude)
            {
                battleEntity = entity;
            }
        }
        return battleEntity;
    }

    public Vector2 Move(EntityUpdateParams param)
    {
        BattleEntity nearestEntity = FindNearestEnemy(param.entities, param.entity.position);
        if (nearestEntity == null)
        {
            nearestEntity = param.player;
        }

        Vector2 moveValue = (nearestEntity.position + new Vector2(0, 300) - param.entity.position).normalized
            * param.timeDiff * speed;
        if (moveValue.x != 0)
        {
            param.entity.facingEast = moveValue.x > 0;
        }
        return moveValue;
    }
}
