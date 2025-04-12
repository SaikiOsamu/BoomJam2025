using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class AttackCollideHandler
{
    public int attack = 5;
    public bool shouldDestructAfterCollide = false;
    public HashSet<BattleEntity> collidedObjects = new HashSet<BattleEntity>(ReferenceEqualityComparer.Instance);

    public AttackCollideHandler(bool shouldDestructAfterCollide, int attack = 5)
    {
        this.shouldDestructAfterCollide = shouldDestructAfterCollide;
        this.attack = attack;
    }

    public void Update(BattleEntity.EntityUpdateParams param, BattleEntity theOtherEntity)
    {
        if (!param.entity.isAlive)
        {
            return;
        }
        if (collidedObjects.Contains(theOtherEntity))
        {
            return;
        }
        if (param.entity.isEnemy == theOtherEntity.isEnemy)
        {
            return;
        }
        theOtherEntity.life -= attack;
        if (shouldDestructAfterCollide)
        {
            param.entity.isAlive = false;
        }
        collidedObjects.Add(theOtherEntity);
    }
}