using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class LifeBasedSelfDestructHandler
{

    public void Update(BattleEntity.EntityUpdateParams param)
    {
        if (param.entity.life <= 0)
        {
            param.entity.isAlive = false;
        }
    }
}