using UnityEngine;

public class ObjectStatusUpdate : MonoBehaviour
{
    public BattleEntity entity;
    public BattleEntity player;
    public LevelManager levelManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (entity != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !entity.isBarrier)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = !entity.facingEast;
            }
            gameObject.transform.localPosition = entity.position;
            gameObject.transform.localRotation = entity.rotation;
            if (!entity.isAlive)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!entity.isProjector || !entity.isAlive)
        {
            return;
        }
        BattleEntity otherEntity = null;
        ObjectStatusUpdate otherComponent = other.gameObject.GetComponent<ObjectStatusUpdate>();
        if (otherComponent != null)
        {
            otherEntity = otherComponent.entity;
        }
        else
        {
            PlayerStatusUpdate playerComponent = other.gameObject.GetComponent<PlayerStatusUpdate>();
            if (playerComponent != null)
            {
                otherEntity = player;
            }
        }
        if (otherEntity == null || otherEntity.isProjector)
        {
            return;
        }
        if (otherEntity.isEnemy == entity.isEnemy)
        {
            return;
        }
        levelManager.RegisterCollision(entity, otherEntity);
    }
}
