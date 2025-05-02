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
using static UnityEngine.EventSystems.EventTrigger;
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
    LEVEL_STAGE_LOST = 4,
    LEVEL_STAGE_WINNER = 5,
    LEVEL_STAGE_ENDLESS = 6,
}

public class LevelManager : MonoBehaviour
{
    public BattleEntity player;
    public BattleEntity boss;
    public BattleEntity? timeExtender;
    public BattleEntity? spaceCutter;
    public List<BattleEntity> entities = new List<BattleEntity>();
    public List<BattleEntity> projectors = new List<BattleEntity>();
    public Dictionary<BattleEntity, CollisionBattleEntity> collisionBattleEntities =
        new Dictionary<BattleEntity, CollisionBattleEntity>(ReferenceEqualityComparer.Instance);
    public float enemySpawnCooldown = 0;
    public int area = 1;
    public int cleanseRewardAlreadyGranted = 0;
    public int cleanse = 0;
    public int cleanseThreshold = 200;
    public int purificationShown = 0;
    public int purificationExpected1 = 0;
    public int purificationExpected2 = 0;
    public float enemySpawnCooldownReset = 0.2f;
    public float bossFightSize = 20;
    public LevelStage levelStage = LevelStage.LEVEL_STAGE_DOING_CLEANSE;
    public bool nearPurificationPoint = false;
    public bool selectingAlly = false;

    [SerializeField]
    public Vector2 bossFightCenter = Vector2.zero;
    [SerializeField]
    private AnimalSelectionUI animalSelectionUI = null;
    [SerializeField]
    private InputActionReference interactionAction;

    [SerializeField]
    private GameObject entityPrefab;
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Character playerPrefab;
    [SerializeField]
    private Character floatingCannonPrefab;
    [SerializeField]
    private Character purificationPointPrefab;
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

    float TimeCoefficient(BattleEntity entity)
    {
        if (timeExtender == null || ReferenceEquals(entity, player))
        {
            return 1.0f;
        }
        return 0.2f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        purificationExpected1 = Random.Range(0, 200);
        purificationExpected2 = Random.Range(0, 200);
        if (purificationExpected1 > purificationExpected2)
        {
            int temp = purificationExpected1;
            purificationExpected1 = purificationExpected2;
            purificationExpected2 = temp;
        }

        player = BattleEntity.FromPrefab(playerPrefab);
        player.dynamicSkills = playerDynamicSkills;

        animalSelectionUI.selectAnimalPartnerDelegate = SpawnAnimalAlly;

        if (floatingCannonPrefab != null)
        {
            BattleEntity floatingCannon = BattleEntity.FromPrefab(floatingCannonPrefab);
            entities.Add(floatingCannon);
            RegisterObject(floatingCannon);
        }

        foreach (Character ally in animalAllyPrefabs)
        {
            BattleEntity a = BattleEntity.FromPrefab(ally);
            entities.Add(a);
            RegisterObject(a);
        }
    }

    void SpawnAnimalAlly(Character prefab)
    {
        BattleEntity ally = BattleEntity.FromPrefab(prefab);
        ally.position = player.position;
        entities.Add(ally);
        RegisterObject(ally);
        selectingAlly = false;
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
        AudioSource audioSource = obj.AddComponent<AudioSource>();
        ObjectStatusUpdate update = obj.AddComponent<ObjectStatusUpdate>();
        update.player = player;
        update.entity = entity;
        update.levelManager = this;
        update.audioSource = audioSource;
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
            else if (status.status?.type == BattleStatusEffectType.DRAG)
            {
                float speed = status.status.pushBackSpeedPerSecond.magnitude;
                moveResult += (status.pullCenter - entity.position) * speed * timeDiff;
            }
            else if (status.status?.type == BattleStatusEffectType.SLOW)
            {
                moveResult *= status.status.slowEffectRatio;
            }
        }
        entity.position += moveResult;
        // Boss fight wall
        if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT && ReferenceEquals(entity, player))
        {
            if (entity.position.x > bossFightCenter.x + bossFightSize)
            {
                entity.position.x = bossFightCenter.x + bossFightSize;
            }
            if (entity.position.x < bossFightCenter.x - bossFightSize)
            {
                entity.position.x = bossFightCenter.x - bossFightSize;
            }
        }
    }

    void HandleAttackResult(List<BattleEntity> newProjectors)
    {
        foreach (BattleEntity entity in newProjectors)
        {
            RegisterObject(entity);
            if (entity.isTimeExtender)
            {
                timeExtender = entity;
            }
            else if (entity.isSpaceCutter)
            {
                spaceCutter = entity;
            }
            else if (entity.isProjector)
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
        bossFightCenter = player.position + new Vector2(13, 0);
        levelStage = LevelStage.LEVEL_STAGE_BOSS_FIGHT;
        boss = BattleEntity.FromPrefab(bossPrefabs[area - 1]);
        boss.isEnemy = true;
        boss.isBoss = true;
        boss.position = player.position + new Vector2(25, 0);
        entities.Add(boss);
        RegisterObject(boss);
    }

    void ShowPurificationPoint()
    {
        BattleEntity purificationPoint = BattleEntity.FromPrefab(purificationPointPrefab);
        purificationPoint.position = player.position + new Vector2(10, 0);
        purificationPoint.selfDestruct = param =>
        {
            if ((param.entity.position - player.position).magnitude < 2)
            {
                nearPurificationPoint = true;
                if (interactionAction.action.triggered && !selectingAlly)
                {
                    player.godPowerMax -= 15;
                    player.godPower = Math.Min(player.godPower, player.godPowerMax);
                    param.entity.isAlive = false;
                    animalSelectionUI.Show();
                    selectingAlly = true;
                }
            }
        };
        entities.Add(purificationPoint);
        RegisterObject(purificationPoint);
        purificationShown += 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (levelStage == LevelStage.LEVEL_STAGE_WINNER)
        {
            return;
        }
        if (selectingAlly)
        {
            return;
        }
        if (!player.isAlive)
        {
            levelStage = LevelStage.LEVEL_STAGE_LOST;
            return;
        }

        if (levelStage == LevelStage.LEVEL_STAGE_DOING_CLEANSE)
        {
            if (cleanse >= cleanseThreshold)
            {
                if (area > bossPrefabs.Count)
                {
                    levelStage = LevelStage.LEVEL_STAGE_WINNER;
                    return;
                }
                levelStage = LevelStage.LEVEL_STAGE_CLEANSE_COMPLETED;
                cleanse = 0;
                cleanseRewardAlreadyGranted = 0;
                purificationExpected1 = Random.Range(0, 200);
                purificationExpected2 = Random.Range(0, 200);
                if (purificationExpected1 > purificationExpected2)
                {
                    int temp = purificationExpected1;
                    purificationExpected1 = purificationExpected2;
                    purificationExpected2 = temp;
                }

                BattleEntity startBossFight = BattleEntity.FromPrefab(startBossFightPrefab);
                startBossFight.isEnemy = true;
                startBossFight.doesNotBeingCutByUltimate = true;
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
            else if (cleanse / (cleanseThreshold / 4) > cleanseRewardAlreadyGranted)
            {
                cleanseRewardAlreadyGranted += 1;
                animalSelectionUI.Show();
                selectingAlly = true;
            }
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
            p.timeDiff = delta * TimeCoefficient(entity);
            if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT)
            {
                p.bossFightCenter = bossFightCenter;
                p.bossFightSize = bossFightSize;
            }
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
            p.timeDiff = delta * TimeCoefficient(entity);
            if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT)
            {
                p.bossFightCenter = bossFightCenter;
                p.bossFightSize = bossFightSize;
            }
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
                // Hidden resolution
                if (victim.isHidden)
                {
                    continue;
                }
                BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
                p.entity = collisionBattleEntity.projector;
                p.entities = entities.AsReadOnly();
                p.player = player;
                p.timeDiff = delta * TimeCoefficient(collisionBattleEntity.projector);
                if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT)
                {
                    p.bossFightCenter = bossFightCenter;
                    p.bossFightSize = bossFightSize;
                }
                collisionBattleEntity.projector.collideHandler(p, victim);
            }
        }
        // Space cut hits everyone.
        if (spaceCutter != null)
        {
            foreach (BattleEntity entity in entities)
            {
                if (!entity.isEnemy)
                {
                    continue;
                }
                BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
                p.entity = spaceCutter;
                p.entities = entities.AsReadOnly();
                p.player = player;
                p.timeDiff = delta * TimeCoefficient(spaceCutter);
                if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT)
                {
                    p.bossFightCenter = bossFightCenter;
                    p.bossFightSize = bossFightSize;
                }
                spaceCutter.collideHandler(p, entity);
            }
        }
        nearPurificationPoint = false;
        // Maybe mark dead
        foreach (BattleEntity entity in entities.Concat(projectors).Prepend(player))
        {
            // Projectors just being cut down by space cutter.
            if (spaceCutter != null)
            {
                if (entity.isProjector && entity.isEnemy && !entity.doesNotBeingCutByUltimate)
                {
                    entity.isAlive = false;
                    continue;
                }
            }
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
            p.timeDiff = delta * TimeCoefficient(entity);
            if (levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT)
            {
                p.bossFightCenter = bossFightCenter;
                p.bossFightSize = bossFightSize;
            }
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
        if (cleanse > purificationExpected1 && purificationShown == 0)
        {
            ShowPurificationPoint();
        }
        if (cleanse > purificationExpected2 && purificationShown == 1)
        {
            ShowPurificationPoint();
        }
        if (timeExtender != null)
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = timeExtender;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta;
            timeExtender.selfDestruct(p);
        }
        if (spaceCutter != null)
        {
            BattleEntity.EntityUpdateParams p = new BattleEntity.EntityUpdateParams();
            p.entity = spaceCutter;
            p.entities = entities.AsReadOnly();
            p.player = player;
            p.timeDiff = delta * TimeCoefficient(spaceCutter);
            spaceCutter.selfDestruct(p);
        }
        // Remove dead objects
        entities.RemoveAll(enemy => !enemy.isAlive);
        foreach (var e in projectors)
        {
            if (!e.isAlive)
            {
                collisionBattleEntities.Remove(e);
            }
        }
        projectors.RemoveAll(proj => !proj.isAlive);
        if (!timeExtender?.isAlive ?? false)
        {
            timeExtender = null;
        }
        if (!spaceCutter?.isAlive ?? false)
        {
            spaceCutter = null;
        }
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
