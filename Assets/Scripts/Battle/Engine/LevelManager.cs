using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    private Character playerPrefab;
    [SerializeField]
    private Character floatingCannonPrefab;
    [SerializeField]
    private List<Character> enemyPrefabs;
    [SerializeField]
    private List<Character> animalAllyPrefabs;
    [SerializeField]
    private List<Skills> playerDynamicSkills;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = BattleEntity.FromPrefab(playerPrefab);
        player.dynamicSkills = playerDynamicSkills;
        entities.Add(player);

        BattleEntity floatingCannon = BattleEntity.FromPrefab(floatingCannonPrefab);
        entities.Add(floatingCannon);
        RegisterObject(floatingCannon);

        // Add animal allies.. For testing.
        foreach (var animalAlly in animalAllyPrefabs)
        {
            BattleEntity ally = BattleEntity.FromPrefab(animalAlly);
            entities.Add(ally);
            RegisterObject(ally);
        }
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
        BattleEntity enemy = BattleEntity.FromPrefab(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]);
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
        foreach (BattleEntity entity in entities.Concat(projectors).Prepend(player))
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
            foreach (BattleEntity victim in collisionBattleEntity.victims.OrderBy(v => v.position.x * (collisionBattleEntity.projector.facingEast ? 1 : -1)))
            {
                // Barrier resolution
                if (victim.isBarrier && collisionBattleEntity.projector.isEnemy)
                {
                    if (collisionBattleEntity.projector.projectorDestroiedOnContactWithBarrier)
                    {
                        // Destroied by barrier.
                        collisionBattleEntity.projector.isAlive = false;
                    }
                    else
                    {
                        // Stop resolution for further contacts.
                        break;
                    }
                }
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
            enemySpawnCooldown = 0.2f;
        }
    }
}
