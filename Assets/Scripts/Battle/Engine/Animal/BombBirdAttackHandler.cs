using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;
using static UnityEngine.EventSystems.EventTrigger;

class BombMoveHandler
{
    float a = 98f;
    float v = 0;

    public Vector2 Move(EntityUpdateParams param)
    {
        v += param.timeDiff * a;
        return new Vector2(0, -v * param.timeDiff);
    }
}

class BombBirdAttackHandler
{
    public float attackCooldown = 0;
    public float attackCooldownWhenAttacked = 1;


    public bool IsNearEnemy(ReadOnlyCollection<BattleEntity> entities, BattleEntity entity)
    {
        foreach (var e in entities)
        {
            if (e.isEnemy == entity.isEnemy)
            {
                continue;
            }
            if (Mathf.Abs(e.position.x - entity.position.x) < 50)
            {
                return true;
            }
        }
        return false;
    }

    public List<BattleEntity> Attack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (attackCooldown > 0)
        {
            attackCooldown -= param.timeDiff;
        }
        else if (IsNearEnemy(param.entities, param.entity))
        {
            BattleEntity bomb = new BattleEntity();
            bomb.position = param.entity.position * 1;
            bomb.color = Color.black;
            bomb.radius = 30;
            bomb.isEnemy = false;
            attackCooldown = attackCooldownWhenAttacked;
            bomb.moveHandler = new BombMoveHandler().Move;
            bomb.attackHandler = BombAttack;
            result.Add(bomb);
        }
        return result;
    }

    private List<BattleEntity> BombAttack(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (param.entity.position.y > 0)
        {
            return result;
        }
        BattleEntity bombExplosion = new BattleEntity();
        bombExplosion.position = param.entity.position * 1;
        bombExplosion.color = Color.cyan;
        bombExplosion.radius = 500;
        bombExplosion.isEnemy = param.entity.isEnemy;
        bombExplosion.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
        bombExplosion.collideHandler = new AttackCollideHandler(false, 5000).Update;
        bombExplosion.isProjector = true;
        result.Add(bombExplosion);

        // Self destruct here.
        param.entity.isAlive = false;

        return result;
    }
}