using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;
using static UnityEngine.EventSystems.EventTrigger;

class BarrierBehavior : BaseBehavior
{
    public override SelfDestructDelegate SelfDestructDelegate => Update;

    BehaviorDefinitions definitions;
    InputAction barrierAction;

    public BarrierBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
        barrierAction = definitions.barrierAction.action;
    }

    private float manaDrain = 0;

    public void Update(EntityUpdateParams param)
    {
        if (!param.entity.isAlive)
        {
            return;
        }
        if (!barrierAction.IsPressed())
        {
            param.entity.isAlive = false;
            return;
        }
        if (param.player.godPower <= 0)
        {
            param.entity.isAlive = false;
            return;
        }
        manaDrain += definitions.godPowerDrainPerSecond * param.timeDiff;
        if (manaDrain > 0)
        {
            int floor = (int)Math.Floor(manaDrain);
            param.player.godPower -= floor;
            manaDrain -= floor;
            if (param.player.godPower <= 0)
            {
                param.entity.isAlive = false;
                param.player.godPower = 0;
                return;
            }
        }
    }
}