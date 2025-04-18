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
    public float damageEvery = -1;
    public float damageLastDealt = 0;
    // Used by damage every N second.
    public int alreadyDamagedObjects = 0;
    public HashSet<BattleEntity> collidedObjects = new HashSet<BattleEntity>(ReferenceEqualityComparer.Instance);

    public AttackCollideHandler(int maxDamageTargets, int attack = 5, float damageEvery = -1)
    {
        this.maxDamageTargets = maxDamageTargets;
        this.attack = attack;
        this.damageEvery = damageEvery;
    }

    public void Update(BattleEntity.EntityUpdateParams param, BattleEntity theOtherEntity)
    {
        if (!param.entity.isAlive)
        {
            return;
        }
        if (damageEvery > 0)
        {
            damageLastDealt += param.timeDiff;
            if (damageLastDealt > damageEvery)
            {
                damageLastDealt = 0;
                alreadyDamagedObjects += collidedObjects.Count;
                collidedObjects.Clear();
            }
        }
        if (collidedObjects.Contains(theOtherEntity))
        {
            return;
        }
        if (param.entity.isEnemy == theOtherEntity.isEnemy)
        {
            return;
        }
        theOtherEntity.Damage(attack);
        collidedObjects.Add(theOtherEntity);
        if (maxDamageTargets > 0 && alreadyDamagedObjects + collidedObjects.Count >= maxDamageTargets)
        {
            param.entity.isAlive = false;
        }
    }
}