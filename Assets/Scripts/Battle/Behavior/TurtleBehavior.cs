using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class TurtleBehavior : BaseBehavior
{
    private BattleEntity player;
    private BehaviorDefinitions definitions;
    private bool hasSetInitialPosition = false;
    private float originX;
    private bool hasInitialized = false;
    private float baseSpeed = 1.5f;
    private float moveDirection = 1f;
    private float minOffset = -2f;
    private float maxOffset = 2f;
    float randomFactor = UnityEngine.Random.Range(0.9f, 1.1f);

    public TurtleBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }

    public override BattleEntity.MoveDelegate MoveDelegate => Move;

    public Vector2 Move(BattleEntity.EntityUpdateParams param)
    {
        if (!hasInitialized)
        {
            originX = param.player.position.x;
            param.entity.position = new Vector2(originX + minOffset, param.player.position.y);
            param.entity.facingEast = true;
            hasInitialized = true;
        }

        Vector2 pos = param.entity.position;

        float speedMultiplier = 0.5f + Mathf.Abs(Mathf.Sin(Time.time * 2f));
        float currentSpeed = definitions.moveSpeed * speedMultiplier;

        float newX = pos.x + currentSpeed * moveDirection * param.timeDiff;

        float offset = newX - originX;

        if (offset > maxOffset)
        {
            offset = maxOffset;
            moveDirection = -1f;
        }
        else if (offset < minOffset)
        {
            offset = minOffset;
            moveDirection = 1f;
        }

        newX = originX + offset;

        param.entity.facingEast = moveDirection > 0;

        return new Vector2(newX - pos.x, 0f);

    }

    private float shieldCooldown = 0;
    public override BattleEntity.AttackDelegate AttackDelegate => Shield;
    public List<BattleEntity> Shield(BattleEntity.EntityUpdateParams param)
    {
        List<BattleEntity> shield = new List<BattleEntity>();

        if (shieldCooldown > 0)
        {
            shieldCooldown -= param.timeDiff;
        }
        else if (param.player.shield < param.player.shieldMax)
        {
            var entitiesSummoned = param.entity.GetSkillSummon(0, out float cooldown);
            foreach (BattleEntity toSummon in entitiesSummoned)
            {
                toSummon.selfDestruct = new TimedProjectionSelfDestructHandler(0.2f).Update;
                shield.Add(toSummon);
            }
            param.player.shield = Math.Min(param.player.shield + definitions.projectileDamage, param.player.shieldMax);
            shieldCooldown = cooldown;
        }

        return shield;
    }
}