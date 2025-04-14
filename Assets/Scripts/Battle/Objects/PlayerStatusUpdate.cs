using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerStatusUpdate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var player = GetComponentInParent<LevelManager>().player;
        transform.position = player.position;
        GetComponent<SpriteRenderer>().flipX = !player.facingEast;
    }
}
