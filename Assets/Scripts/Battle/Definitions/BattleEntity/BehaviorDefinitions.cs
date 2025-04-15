using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorDefinitions", menuName = "Battle Entities/BehaviorDefinitions")]
public class BehaviorDefinitions : ScriptableObject
{
    public BehaviorType behaviorType = BehaviorType.Unknown;
    public float moveSpeed = 0.5f;
}
