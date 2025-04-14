using UnityEngine;

public class ObjectStatusUpdate : MonoBehaviour
{
    public BattleEntity entity;
    public BattleEntity player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (entity != null)
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = !entity.facingEast;
            if (player != null)
            {
                gameObject.transform.localPosition = entity.position;
            }
            if (!entity.isAlive)
            {
                Destroy(gameObject);
            }
        }
    }
}
