using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum BattleStatusEffectType
{
    UNKNOWN = 0,
    PUSH_BACK = 1,
    POISON = 2,
    SLOW = 3,
}

[CreateAssetMenu(fileName = "BattleStatusEffect", menuName = "Battle Entities/Battle Status Effect")]
public class BattleStatusEffect : ScriptableObject
{
    public BattleStatusEffectType type = BattleStatusEffectType.UNKNOWN;
    public string statusName;
    public string description;
    public int maxAppliedAtOnce = 0;
    public float statusEffectTime = 0;
    public Vector2 pushBackSpeedPerSecond = Vector2.zero;
    public float poisonDamagePerSecond = 0;
    public float slowEffectRatio = 1.0f;
}
