using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Battle Entities/Skills")]
public class Skills : ScriptableObject
{
    public string skillName;
    public string description;
    public List<Character> summoning;
    public float cooldownSecond = 0;
}
