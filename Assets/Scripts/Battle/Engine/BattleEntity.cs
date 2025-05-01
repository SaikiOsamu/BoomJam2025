using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;

public class BattleStatus
{
    public BattleStatusEffect status = null;
    public Vector2 pullCenter = Vector2.zero;
    public bool pushBackFacingEast = true;
    public float damageToApply = 0;
    public float timeElapsed = 0;
}

public class BattleEntity
{
    public delegate Vector2 MoveDelegate(EntityUpdateParams param);
    public delegate List<BattleEntity> AttackDelegate(EntityUpdateParams param);
    public delegate bool CollideDelegate(EntityUpdateParams param, BattleEntity theOtherEntity);
    public delegate void SelfDestructDelegate(EntityUpdateParams param);

    public class EntityUpdateParams
    {
        public BattleEntity entity;
        public ReadOnlyCollection<BattleEntity> entities;
        public BattleEntity player;
        public float timeDiff;
        public Vector2? bossFightCenter = null;
        public float? bossFightSize = null;
    }
    public Character prefabCharacter = null;
    public Vector2 position = Vector2.zero;
    public Quaternion rotation = Quaternion.identity;
    public Color color = Color.white;
    public Sprite sprite = null;
    public float radius = 1;
    public int life = 100;
    public int lifeMax = 100;
    public int resilience = 0;
    public int resilienceMax = 0;
    public int godPower = 100;
    public int godPowerMax = 100;
    public float godPowerRecoveryPerSecond = 0.1f;
    public int shield = 0;
    public int shieldMax = 200;
    public bool facingEast = true;
    public bool isAlive = true;
    public bool isEnemy = false;
    public bool isProjector = false;
    public bool isTimeExtender = false;
    public bool isSpaceCutter = false;
    public bool doesNotBeingCutByUltimate = false;
    public bool isAttacking = false;
    public bool projectorDestroiedOnContactWithBarrier = false;
    public bool isBarrier = false;
    public int cleanseWhenDefeated = 0;
    // Only affects collide.
    public bool isHidden = false;
    public List<Skills> dynamicSkills = new List<Skills>();
    public List<BattleStatus> statusInEffect = new List<BattleStatus>();
    public MoveDelegate moveHandler = _ => Vector2.zero;
    public AttackDelegate attackHandler = _ => new List<BattleEntity>();
    public CollideDelegate collideHandler = (_, _) => false;
    public SelfDestructDelegate selfDestruct = _ => { };

    public static BattleEntity FromPrefab(Character prefabCharacter)
    {
        BattleEntity battleEntity = new BattleEntity();
        battleEntity.prefabCharacter = prefabCharacter;
        battleEntity.isProjector = prefabCharacter.isProjector;
        battleEntity.life = prefabCharacter.life;
        battleEntity.lifeMax = prefabCharacter.lifeMax;
        battleEntity.resilience = prefabCharacter.resilience;
        battleEntity.resilienceMax = prefabCharacter.resilienceMax;
        battleEntity.godPower = prefabCharacter.godPower;
        battleEntity.godPowerMax = prefabCharacter.godPowerMax;
        battleEntity.godPowerRecoveryPerSecond = prefabCharacter.godPowerRecoveryPerSecond;
        battleEntity.shield = prefabCharacter.shield;
        battleEntity.shieldMax = prefabCharacter.shieldMax;
        battleEntity.projectorDestroiedOnContactWithBarrier = prefabCharacter.projectorDestroiedOnContactWithBarrier;
        battleEntity.isBarrier = prefabCharacter.isBarrier;
        battleEntity.cleanseWhenDefeated = prefabCharacter.cleanseWhenDefeated;
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

    public Skills GetSkill(int skillIndex, bool dynamic)
    {
        Skills skill = null;
        if (dynamic)
        {
            if (dynamicSkills.Count > skillIndex)
            {
                skill = dynamicSkills[skillIndex];
            }
        }
        else
        {
            if (prefabCharacter != null && prefabCharacter.skills.Count > skillIndex)
            {
                skill = prefabCharacter.skills[skillIndex];
            }
        }
        return skill;
    }

    public List<BattleEntity> GetSkillSummon(int skillIndex, out float cooldown, bool dynamic = false)
    {
        return GetSkillSummon(skillIndex, out cooldown, out _, dynamic);
    }

    public List<BattleEntity> GetSkillSummon(int skillIndex, out float cooldown, out int godPowerConsumption, bool dynamic = false)
    {
        List<BattleEntity> result = new List<BattleEntity>();
        Skills skill = GetSkill(skillIndex, dynamic);
        if (skill == null)
        {
            cooldown = 0;
            godPowerConsumption = 0;
            return result;
        }
        cooldown = skill.cooldownSecond;
        godPowerConsumption = skill.godPowerConsumption;
        foreach (Character summoning in skill.summoning)
        {
            BattleEntity toSummon = FromPrefab(summoning);
            toSummon.position = position * 1;
            toSummon.facingEast = facingEast;
            toSummon.isEnemy = isEnemy;
            result.Add(toSummon);
        }
        return result;
    }

    public float GetSkillCasttime(int skillIndex, bool dynamic = false)
    {
        Skills skill = GetSkill(skillIndex, dynamic);
        if (skill == null)
        {
            return 0;
        }
        return skill.castSecond;
    }

    // Deal x damage to this entity.
    public void Damage(int x)
    {
        if (shield > 0)
        {
            if (x > shield)
            {
                x -= shield;
                shield = 0;
            }
            else
            {
                shield -= x;
                return;
            }
        }
        life -= x;
    }
}
