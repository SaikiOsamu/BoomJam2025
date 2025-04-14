using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

public class CollisionBattleEntity
{
    public BattleEntity projector;
    public List<BattleEntity> victims;
}

public class LevelManager : MonoBehaviour
{
    public BattleEntity player;
    public List<BattleEntity> entities = new List<BattleEntity>();
    public List<BattleEntity> projectors = new List<BattleEntity>();
    public Dictionary<BattleEntity, CollisionBattleEntity> collisionBattleEntities =
        new Dictionary<BattleEntity, CollisionBattleEntity>(ReferenceEqualityComparer.Instance);
    public float enemySpawnCooldown = 0;

    [SerializeField]
    private GameObject entityPrefab;
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Character birdPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new BattleEntity();
        player.moveHandler = new PlayerMoveHandler(InputSystem.actions.FindAction("Move"),
            InputSystem.actions.FindAction("Jump")).Move;
        player.attackHandler = new PlayerAttackHandler(InputSystem.actions.FindAction("Attack"),
            InputSystem.actions.FindAction("Skill 1")).Attack;
        player.selfDestruct = new LifeBasedSelfDestructHandler().Update;

        // Add a bomb bird.
        BattleEntity bombBird = BattleEntity.FromPrefab(birdPrefab);
        BombBirdHandler bombBirdHandler = new BombBirdHandler();
        bombBird.moveHandler = bombBirdHandler.Move;
        bombBird.attackHandler = bombBirdHandler.Attack;
        entities.Add(bombBird);
        RegisterObject(bombBird);
    }

    void RegisterObject(BattleEntity entity)
    {
        GameObject obj;
        if (entity.prefabCharacter != null)
        {
            obj = Instantiate(entity.prefabCharacter.prefab);
        }
        else
        {
            if (entity.isProjector)
            {
                obj = Instantiate(projectilePrefab);
            }
            else
            {
                obj = Instantiate(entityPrefab);
            }
            obj.transform.localScale = Vector3.one * entity.radius;
            if (entity.sprite != null)
            {
                obj.GetComponent<SpriteRenderer>().sprite = entity.sprite;
            }
            obj.GetComponent<SpriteRenderer>().color = entity.color;
        }
        ObjectStatusUpdate update = obj.AddComponent<ObjectStatusUpdate>();
        update.player = player;
        update.entity = entity;
        update.levelManager = this;
    }

    void HandleMoveResult(BattleEntity entity, Vector2 moveResult)
    {
        entity.position += moveResult;
    }

    void HandleAttackResult(List<BattleEntity> newProjectors)
    {
        foreach (BattleEntity entity in newProjectors)
        {
            RegisterObject(entity);
            if (entity.isProjector)
            {
                projectors.Add(entity);
            }
            else
            {
                entities.Add(entity);
            }
        }
    }

    public void RegisterCollision(BattleEntity projectile, BattleEntity victim)
    {
        if (!collisionBattleEntities.ContainsKey(projectile))
        {
            CollisionBattleEntity collisionBattleEntity = new CollisionBattleEntity();
            collisionBattleEntity.projector = projectile;
            collisionBattleEntity.victims = new List<BattleEntity> { victim };
            collisionBattleEntities.Add(projectile, collisionBattleEntity);
        }
        else
        {
            collisionBattleEntities[projectile].victims.Add(victim);
        }
    }

    void AddEnemy()
    {
        BattleEntity enemy = new BattleEntity();
        enemy.radius = 1;
        enemy.isEnemy = true;
        enemy.moveHandler = new ChasePlayerMoveHandler(player, 0.5f).Move;
        enemy.attackHandler = new NearPlayerAttackHandler(player).Attack;
        enemy.selfDestruct = new LifeBasedSelfDestructHandler().Update;
        enemy.color = Color.red;
        float position = Random.value;
        if (position > 0.5)
        {
            enemy.position = new Vector2(7, 0);
        }
        else
        {
            enemy.position = new Vector2(-7, 0);
        }
        entities.Add(enemy);
        RegisterObject(enemy);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isAlive)
        {
            return;
        }
        float delta = Time.deltaTime;
        // Objects Move
        foreach (BattleEntity entity in entities.Concat(projectors).Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            HandleMoveResult(entity, entity.moveHandler(p));
        }
        // Objects Attack
        List<BattleEntity> attackResults = new List<BattleEntity>();
        foreach (BattleEntity entity in entities.Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            attackResults.AddRange(entity.attackHandler(p));
        }
        HandleAttackResult(attackResults);
        // Projects Collide
        foreach (CollisionBattleEntity collisionBattleEntity in collisionBattleEntities.Values)
        {
            // TODO: Sort the victims to support shield.
            foreach (BattleEntity victim in collisionBattleEntity.victims)
            {
                BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
                p.entity = collisionBattleEntity.projector;
                p.entities = entities.AsReadOnly();
                p.player = player;
                p.timeDiff = delta;
                collisionBattleEntity.projector.collideHandler(p, victim);
            }
        }
        // Maybe mark dead
        foreach (BattleEntity entity in entities.Concat(projectors).Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            entity.selfDestruct(p);
        }
        // Remove dead objects
        entities.RemoveAll(enemy => !enemy.isAlive);
        projectors.RemoveAll(proj => !proj.isAlive);
        // Add enemy
        if (enemySpawnCooldown > 0)
        {
            enemySpawnCooldown -= delta;
        }
        else
        {
            AddEnemy();
            enemySpawnCooldown = Mathf.Min(player.position.x != 0 ? 10 / Mathf.Abs(player.position.x) : 10, 10);
        }
    }
}
