using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BattleEntity
{
    public delegate Vector2 MoveDelegate(EntityUpdateParams param);
    public delegate List<BattleEntity> AttackDelegate(EntityUpdateParams param);
    public delegate void CollideDelegate(EntityUpdateParams param, BattleEntity theOtherEntity);
    public delegate void SelfDestructDelegate(EntityUpdateParams param);

    public class EntityUpdateParams
    {
        public BattleEntity entity;
        public ReadOnlyCollection<BattleEntity> entities;
        public BattleEntity player;
        public float timeDiff;
    }
    public Character prefabCharacter = null;
    public Vector2 position = Vector2.zero;
    public Quaternion rotation = Quaternion.identity;
    public Color color = Color.white;
    public Sprite sprite = null;
    public float radius = 1;
    public int life = 100;
    public int resilience = 0;
    public int resilienceMax = 0;
    public int shield = 0;
    public int shieldMax = 200;
    public bool facingEast = true;
    public bool isAlive = true;
    public bool isEnemy = false;
    public bool isProjector = false;
    public MoveDelegate moveHandler = _ => Vector2.zero;
    public AttackDelegate attackHandler = _ => new List<BattleEntity>();
    public CollideDelegate collideHandler = (_, _) => { };
    public SelfDestructDelegate selfDestruct = _ => { };

    public static BattleEntity FromPrefab(Character prefabCharacter)
    {
        BattleEntity battleEntity = new BattleEntity();
        battleEntity.prefabCharacter = prefabCharacter;
        battleEntity.isProjector = prefabCharacter.isProjector;
        battleEntity.life = prefabCharacter.life;
        battleEntity.resilience = prefabCharacter.resilience;
        battleEntity.resilienceMax = prefabCharacter.resilienceMax;
        battleEntity.shield = prefabCharacter.shield;
        battleEntity.shieldMax = prefabCharacter.shieldMax;
        if (prefabCharacter.behavior != null)
        {
            Behavior behavior = BehaviorFactory.GetBehavior(prefabCharacter.behavior);
            battleEntity.moveHandler = behavior.MoveDelegate;
            battleEntity.attackHandler = behavior.AttackDelegate;
            battleEntity.collideHandler = behavior.CollideDelegate;
            battleEntity.selfDestruct = behavior.SelfDestructDelegate;
        }
        return battleEntity;
    }

    public List<BattleEntity> GetSkillSummon(int skillIndex)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        if (prefabCharacter == null)
        {
            return result;
        }
        if (prefabCharacter.skills.Count <= skillIndex)
        {
            return result;
        }
        foreach (Character summoning in prefabCharacter.skills[skillIndex].summoning)
        {
            BattleEntity toSummon = FromPrefab(summoning);
            toSummon.position = position * 1;
            toSummon.isEnemy = isEnemy;
            result.Add(toSummon);
        }
        return result;
    }
}
