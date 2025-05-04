using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CameraPlayerStatusReverseUpdate : MonoBehaviour
{
    [SerializeField]
    private LevelManager levelManager = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private Vector3 target = Vector3.zero;
    [SerializeField]
    private float speed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        var player = levelManager?.player;
        if (player == null)
        {
            return;
        }
        if (levelManager.levelStage != LevelStage.LEVEL_STAGE_BOSS_FIGHT)
        {
            target = new Vector3(player.position.x + 3, 0, -10);
            if ((transform.localPosition - target).magnitude < speed * Time.deltaTime)
            {
                transform.localPosition = target;
            }
            else
            {
                transform.localPosition += (target - transform.localPosition).normalized * speed * Time.deltaTime;
            }
        }
    }
}
