using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField]
    private Character enemyPrefab;
    [SerializeField]
    private Character floatingCannonPrefab;

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
        //BattleEntity bombBird = BattleEntity.FromPrefab(birdPrefab);
        //entities.Add(bombBird);
        //RegisterObject(bombBird);
        // Add a Floating cannon object.
        BattleEntity floatingCannon = BattleEntity.FromPrefab(floatingCannonPrefab);
        entities.Add(floatingCannon);
        RegisterObject(floatingCannon);
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
        BattleEntity enemy = BattleEntity.FromPrefab(enemyPrefab);
        enemy.isEnemy = true;
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
