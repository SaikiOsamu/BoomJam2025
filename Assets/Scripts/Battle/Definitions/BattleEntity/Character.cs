using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Battle Entities/Character")]
public class Character : ScriptableObject
{
    public string id;
    public string entityName;
    public string description;
    public bool isProjector;
    public bool projectorDestroiedOnContactWithBarrier;
    public bool isBarrier;
    public GameObject prefab;
    public List<Skills> skills;
    public int life = 100;
    public int lifeMax = 100;
    public int lifeHealPerSecond = 0;
    public int resilience = 0;
    public int resilienceMax = 0;
    public int godPower = 100;
    public int godPowerMax = 100;
    public int cleanseWhenDefeated = 1;
    public float godPowerRecoveryPerSecond = 0.1f;
    public int shield = 0;
    public int shieldMax = 200;
    public BehaviorDefinitions behavior;
    public Sprite icon;
    public AudioClip onSpawn;
    public AudioClip onDespawn;
}
