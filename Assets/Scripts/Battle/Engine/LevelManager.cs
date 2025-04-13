using NUnit.Framework;
using System.Collections.Generic;
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
        public float timeDiff;
    }

    public Vector2 position = Vector2.zero;
    public Color color = Color.white;
    public Sprite sprite = null;
    public float radius = 100;
    public int life = 100;
    public int shield = 0;
    public int shieldMax = 200;
    public bool facingEast = true;
    public bool isAlive = true;
    public bool isEnemy = false;
    public MoveDelegate moveHandler = _ => Vector2.zero;
    public AttackDelegate attackHandler = _ => new List<BattleEntity>();
    public CollideDelegate collideHandler = (_, _) => { };
    public SelfDestructDelegate selfDestruct = _ => { };
}

public class LevelManager : MonoBehaviour
{
    public BattleEntity player;
    public List<BattleEntity> enemies = new List<BattleEntity>();
    public List<BattleEntity> projectors = new List<BattleEntity>();
    public float enemySpawnCooldown = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new BattleEntity();
        player.moveHandler = new PlayerMoveHandler(InputSystem.actions.FindAction("Move")).Move;
        player.attackHandler = new PlayerAttackHandler(InputSystem.actions.FindAction("Attack"),
            InputSystem.actions.FindAction("Skill 1")).Attack;
        player.selfDestruct = new LifeBasedSelfDestructHandler().Update;
    }

    void RegisterObject(BattleEntity entity, Color color)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.AddComponent<CanvasRenderer>();
        obj.AddComponent<RectTransform>().sizeDelta = new Vector2(entity.radius, entity.radius);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        ObjectStatusUpdate update = obj.AddComponent<ObjectStatusUpdate>();
        update.player = player;
        update.entity = entity;
    }

    void RegisterObject(BattleEntity entity, Sprite sprite)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.AddComponent<CanvasRenderer>();
        obj.AddComponent<RectTransform>().sizeDelta = new Vector2(entity.radius, entity.radius);
        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
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
            if (entity.sprite != null)
            {
                RegisterObject(entity, entity.sprite);
            }
            else
            {
                RegisterObject(entity, entity.color);
            }
            projectors.Add(entity);
        }
    }

    bool Collided(BattleEntity entity, BattleEntity entity2)
    {
        return (entity.position - entity2.position).magnitude * 2 < (entity.radius + entity2.radius);
    }
    void AddEnemy()
    {
        BattleEntity enemy = new BattleEntity();
        enemy.radius = 100;
        enemy.isEnemy = true;
        enemy.moveHandler = new ChasePlayerMoveHandler(player, 50).Move;
        enemy.attackHandler = new NearPlayerAttackHandler(player).Attack;
        enemy.selfDestruct = new LifeBasedSelfDestructHandler().Update;
        float position = Random.value;
        if (position > 0.5)
        {
            enemy.position = new Vector2(700, 0);
        }
        else
        {
            enemy.position = new Vector2(-700, 0);
        }
        enemies.Add(enemy);
        RegisterObject(enemy, Color.red);
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
        foreach (BattleEntity entity in enemies.Concat(projectors).Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.timeDiff = delta;
            HandleMoveResult(entity, entity.moveHandler(p));
        }
        // Objects Attack
        foreach (BattleEntity entity in enemies.Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.timeDiff = delta;
            HandleAttackResult(entity.attackHandler(p));
        }
        // Projects Collide
        foreach (BattleEntity project in projectors)
        {
            foreach (BattleEntity entity in enemies.Prepend(player))
            {
                if (Collided(project, entity))
                {
                    BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
                    p.entity = project;
                    p.timeDiff = delta;
                    project.collideHandler(p, entity);

                }
            }
        }
        // Maybe mark dead
        foreach (BattleEntity entity in enemies.Concat(projectors).Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.timeDiff = delta;
            entity.selfDestruct(p);
        }
        // Remove dead objects
        enemies.RemoveAll(enemy => !enemy.isAlive);
        projectors.RemoveAll(proj => !proj.isAlive);
        // Add enemy
        if (enemySpawnCooldown > 0)
        {
            enemySpawnCooldown -= delta;
        }
        else
        {
            AddEnemy();
            enemySpawnCooldown = Mathf.Min(player.position.x != 0 ? 1000 / Mathf.Abs(player.position.x) : 10, 10);
        }
    }
}
