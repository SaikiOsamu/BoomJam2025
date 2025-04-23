using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CameraPlayerStatusReverseUpdate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var player = transform.parent.gameObject.GetComponentInParent<LevelManager>().player;
        transform.localPosition = new Vector3(8, -player.position.y, -10);
    }
}
