using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CollisionBattleEntity
{
    public BattleEntity projector;
    public List<BattleEntity> victims;
}

[Serializable]
public enum LevelStage
{
    LEVEL_STAGE_UNKNOWN = 0,
    LEVEL_STAGE_DOING_CLEANSE = 1,
    LEVEL_STAGE_CLEANSE_COMPLETED = 2,
    LEVEL_STAGE_BOSS_FIGHT = 3,
}

public class LevelManager : MonoBehaviour
{
    public BattleEntity player;
    public BattleEntity boss;
    public List<BattleEntity> entities = new List<BattleEntity>();
    public List<BattleEntity> projectors = new List<BattleEntity>();
    public Dictionary<BattleEntity, CollisionBattleEntity> collisionBattleEntities =
        new Dictionary<BattleEntity, CollisionBattleEntity>(ReferenceEqualityComparer.Instance);
    public float enemySpawnCooldown = 0;
    public int area = 1;
    public int cleanse = 0;
    public int cleanseThreshold = 200;
    public float enemySpawnCooldownReset = 0.2f;
    public LevelStage levelStage = LevelStage.LEVEL_STAGE_DOING_CLEANSE;

    [SerializeField]
    private GameObject entityPrefab;
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Character playerPrefab;
    [SerializeField]
    private Character floatingCannonPrefab;
    [SerializeField]
    private Character startBossFightPrefab;
    [SerializeField]
    private List<Character> enemyPrefabs;
    [SerializeField]
    private List<Character> bossPrefabs;
    [SerializeField]
    private List<Character> animalAllyPrefabs;
    [SerializeField]
    private List<Skills> playerDynamicSkills;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = BattleEntity.FromPrefab(playerPrefab);
        player.dynamicSkills = playerDynamicSkills;

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

    void HandleMoveResult(
        BattleEntity entity,
        Vector2 moveResult,
        float timeDiff,
        Dictionary<string, int> statusTakenEffectMap)
    {
        foreach (BattleStatus status in entity.statusInEffect)
        {
            if (status.status?.maxAppliedAtOnce > 0)
            {
                int appliedCount = statusTakenEffectMap.GetValueOrDefault(status.status?.name ?? "");
                if (appliedCount >= status.status?.maxAppliedAtOnce)
                {
                    continue;
                }
                statusTakenEffectMap[status.status?.name ?? ""] = appliedCount + 1;
            }
            if (status.status?.type == BattleStatusEffectType.PUSH_BACK)
            {
                Vector2 pushBack = status.status.pushBackSpeedPerSecond * timeDiff;
                if (!status.pushBackFacingEast)
                {
                    pushBack.x *= -1;
                }
                moveResult += pushBack;
            }
            else if (status.status?.type == BattleStatusEffectType.SLOW)
            {
                moveResult *= status.status.slowEffectRatio;
            }
        }
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
        enemy.position = player.position + new Vector2(25, 0);
        entities.Add(enemy);
        RegisterObject(enemy);
    }

    void EnterBossFight()
    {
        if (levelStage != LevelStage.LEVEL_STAGE_CLEANSE_COMPLETED)
        {
            return;
        }
        levelStage = LevelStage.LEVEL_STAGE_BOSS_FIGHT;
        boss = BattleEntity.FromPrefab(bossPrefabs[area - 1]);
        boss.isEnemy = true;
        boss.position = player.position + new Vector2(25, 0);
        entities.Add(boss);
        RegisterObject(boss);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isAlive)
        {
            return;
        }

        if (levelStage == LevelStage.LEVEL_STAGE_DOING_CLEANSE && cleanse >= cleanseThreshold)
        {
            levelStage = LevelStage.LEVEL_STAGE_CLEANSE_COMPLETED;
            cleanse = 0;

            BattleEntity startBossFight = BattleEntity.FromPrefab(startBossFightPrefab);
            startBossFight.isEnemy = true;
            startBossFight.position = player.position + new Vector2(10, 0);
            startBossFight.collideHandler = (param, other) =>
            {
                if (!param.entity.isAlive)
                {
                    return false;
                }
                if (!ReferenceEquals(other, player))
                {
                    return false;
                }
                EnterBossFight();
                return true;
            };
            startBossFight.selfDestruct = param =>
            {
                if (levelStage != LevelStage.LEVEL_STAGE_CLEANSE_COMPLETED)
                {
                    param.entity.isAlive = false;
                }
            };
            projectors.Add(startBossFight);
            RegisterObject(startBossFight);
        }
        if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT && !(boss?.isAlive ?? true))
        {
            levelStage = LevelStage.LEVEL_STAGE_DOING_CLEANSE;
            area += 1;
            if (area == 4)
            {
                // TODO: Win?
            }
        }
        float delta = Time.deltaTime;
        // Objects Move
        Dictionary<string, int> statusTakenEffectMap = new Dictionary<string, int>();
        foreach (BattleEntity entity in entities.Concat(projectors).Prepend(player))
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            HandleMoveResult(entity, entity.moveHandler(p), delta, statusTakenEffectMap);
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
            foreach (BattleStatus status in entity.statusInEffect)
            {
                // Progress the time.
                status.timeElapsed += delta;
                if (status.status?.maxAppliedAtOnce > 0)
                {
                    int appliedCount = statusTakenEffectMap.GetValueOrDefault(status.status?.name ?? "");
                    if (appliedCount >= status.status?.maxAppliedAtOnce)
                    {
                        continue;
                    }
                    statusTakenEffectMap[status.status?.name ?? ""] = appliedCount + 1;
                }
                // Apply damage related status.
                if (status.status?.type == BattleStatusEffectType.POISON)
                {
                    status.damageToApply += delta * status.status?.poisonDamagePerSecond ?? 0;
                    if (status.damageToApply > 0)
                    {
                        int damageToApply = Mathf.FloorToInt(status.damageToApply);
                        entity.life -= damageToApply;
                        status.damageToApply -= damageToApply;
                    }
                }
            }
            // Remove if effect no longer takes effect.
            entity.statusInEffect.RemoveAll(s => s.timeElapsed > (s.status?.statusEffectTime ?? 0));
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = entity;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            entity.selfDestruct(p);
            // Add cleanse progress if needed.
            if (entity.isEnemy && 
                !entity.isProjector && 
                !entity.isAlive && 
                levelStage == LevelStage.LEVEL_STAGE_DOING_CLEANSE)
            {
                cleanse += entity.cleanseWhenDefeated;
            }
        }
        // Remove dead objects
        entities.RemoveAll(enemy => !enemy.isAlive);
        projectors.RemoveAll(proj => !proj.isAlive);
        // Add enemy
        if (enemySpawnCooldown > 0)
        {
            enemySpawnCooldown -= delta;
        }
        else if (levelStage == LevelStage.LEVEL_STAGE_DOING_CLEANSE)
        {
            AddEnemy();
            enemySpawnCooldown = enemySpawnCooldownReset;
        }
    }
}
