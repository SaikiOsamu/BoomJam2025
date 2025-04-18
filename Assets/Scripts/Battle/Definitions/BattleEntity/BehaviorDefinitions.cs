using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BehaviorDefinitions", menuName = "Battle Entities/BehaviorDefinitions")]
public class BehaviorDefinitions : ScriptableObject
{
    public BehaviorType behaviorType = BehaviorType.Unknown;
    // Used by PlayerBehavior
    public InputActionProperty moveAction;
    public InputActionProperty jumpAction;
    public InputActionProperty blinkAction;
    public InputActionProperty attackAction;
    public InputActionProperty skill1Action;
    public InputActionProperty barrierAction;
    public float moveSpeed = 0.5f;
    // Set this to a non positive number to indicate infinite.
    public int maxDamageTargets = -1;
    public int projectileDamage = 0;
    // Set this to a negative number to indicate no destruction by ttl.
    public float timeToLive = 1;
    public float godPowerDrainPerSecond = 0;
    // For ranged enemy, this is the distance to player for attack etc.
    public float attackDistance = 0;
}
