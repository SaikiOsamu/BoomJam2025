using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Battle Entities/Character")]
public class Character : ScriptableObject
{
    public string entityName;
    public string description;
    public bool isProjector;
    public GameObject prefab;
    public List<Skills> skills;
    public int life = 100;
    public int resilience = 0;
    public int resilienceMax = 0;
    public int shield = 0;
    public int shieldMax = 200;
}
