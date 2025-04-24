using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CameraBossFightUpdate : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private LevelManager levelManager = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        animator?.SetBool("is_boss_fight", levelManager?.levelStage == LevelStage.LEVEL_STAGE_BOSS_FIGHT);
    }
}
