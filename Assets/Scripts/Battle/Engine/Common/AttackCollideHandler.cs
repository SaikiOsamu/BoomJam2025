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
    public int maxDamageTargets = -1;
    public HashSet<BattleEntity> collidedObjects = new HashSet<BattleEntity>(ReferenceEqualityComparer.Instance);

    public AttackCollideHandler(int maxDamageTargets, int attack = 5)
    {
        this.maxDamageTargets = maxDamageTargets;
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
        if (maxDamageTargets > 0 && collidedObjects.Count >= maxDamageTargets)
        {
            param.entity.isAlive = false;
        }
        collidedObjects.Add(theOtherEntity);
    }
}