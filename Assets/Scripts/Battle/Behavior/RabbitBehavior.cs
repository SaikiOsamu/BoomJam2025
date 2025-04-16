using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class RabbitBehavior : BaseBehavior
{
    private BattleEntity player;
    private bool hasSetInitialPosition = false;
    private float originX;
    private bool hasInitialized = false;
    private float baseSpeed = 1.5f;  
    private float moveDirection = 1f;
    private float minOffset = -2f;
    private float maxOffset = 2f;
    public float defaultMoveSpeed = 1;
    float randomFactor = UnityEngine.Random.Range(0.9f, 1.1f);

    public RabbitBehavior(BehaviorDefinitions definitions)
    {
        defaultMoveSpeed = definitions.moveSpeed;
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
        float currentSpeed = baseSpeed * speedMultiplier;

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

    private float healCooldown = 2f;
    private float healTimer = 0f;
    private int minHeal = 20;
    private int maxHeal = 80;
    public override BattleEntity.AttackDelegate AttackDelegate => Heal;
    public List<BattleEntity> Heal(BattleEntity.EntityUpdateParams param)
    {
        healTimer += param.timeDiff;
        List<BattleEntity> healed = new List<BattleEntity>();

        if (healTimer >= healCooldown)
        {
            healTimer = 0f;

            BattleEntity player = param.player;
            if (player.life < player.lifeMax)
            {
                int healAmount = Random.Range(minHeal, maxHeal + 1); 
                player.life = Mathf.Min(player.life + healAmount, player.lifeMax);
                healed.Add(player);

                Debug.Log($"兔子给玩家加了 {healAmount} 点血！");
            }
        }

        return healed;
    }
}