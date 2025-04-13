using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

class TimedProjectionSelfDestructHandler
{
    public float timeToLive = 0;
    public TimedProjectionSelfDestructHandler(float timeToLive)
    {
        this.timeToLive = timeToLive;
    }

    public void Update(BattleEntity.EntityUpdateParams param)
    {
        timeToLive -= param.timeDiff;
        if (timeToLive <= 0)
        {
            param.entity.isAlive = false;
        }
    }
}