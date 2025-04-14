using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public Vector2 position = Vector2.zero;
    public Color color = Color.white;
    public Sprite sprite = null;
    public float radius = 1;
    public int life = 100;
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
}

public class LevelManager : MonoBehaviour
{
    public BattleEntity player;
    public List<BattleEntity> entities = new List<BattleEntity>();
    public List<BattleEntity> projectors = new List<BattleEntity>();
    public float enemySpawnCooldown = 0;

    [SerializeField]
    private GameObject entityPrefab;
    [SerializeField]
    private GameObject projectilePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new BattleEntity();
        player.moveHandler = new PlayerMoveHandler(InputSystem.actions.FindAction("Move")).Move;
        player.attackHandler = new PlayerAttackHandler(InputSystem.actions.FindAction("Attack"),
            InputSystem.actions.FindAction("Skill 1")).Attack;
        player.selfDestruct = new LifeBasedSelfDestructHandler().Update;

        // Add a bomb bird.
        BattleEntity bombBird = new BattleEntity();
        bombBird.color = Color.green;
        BombBirdHandler bombBirdHandler = new BombBirdHandler();
        bombBird.moveHandler = bombBirdHandler.Move;
        bombBird.attackHandler = bombBirdHandler.Attack;
        entities.Add(bombBird);
        RegisterObject(bombBird);
    }

    void RegisterObject(BattleEntity entity)
    {
        GameObject obj;
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
        ObjectStatusUpdate update = obj.AddComponent<ObjectStatusUpdate>();
        update.player = player;
        update.entity = entity;
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

    bool Collided(BattleEntity entity, BattleEntity entity2)
    {
        return (entity.position - entity2.position).magnitude * 2 < (entity.radius + entity2.radius);
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
        foreach (BattleEntity project in projectors)
        {
            foreach (BattleEntity entity in entities.Prepend(player))
            {
                if (Collided(project, entity))
                {
                    BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
                    p.entity = project;
                    p.entities = entities.AsReadOnly();
                    p.player = player;
                    p.timeDiff = delta;
                    project.collideHandler(p, entity);

                }
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
