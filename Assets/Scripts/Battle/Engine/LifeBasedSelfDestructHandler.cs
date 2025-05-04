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
    public float lifeToHeal = 0;

    public void Update(BattleEntity.EntityUpdateParams param)
    {
        lifeToHeal += param.entity.lifeHealPerSecond * param.timeDiff;
        if (lifeToHeal > 1)
        {
            int healed = Mathf.FloorToInt(lifeToHeal);
            param.entity.life = Mathf.Min(param.entity.life + healed, param.entity.lifeMax);
            lifeToHeal -= healed;
        }

        if (param.entity.life <= 0)
        {
            param.entity.isAlive = false;
        }
    }
}