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
        if (player != null)
        {
            if (player.facingEast)
            {
                gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
            else
            {
                gameObject.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}
