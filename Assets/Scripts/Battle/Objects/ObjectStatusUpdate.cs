using UnityEngine;

public class ObjectStatusUpdate : MonoBehaviour
{
    public BattleEntity entity;
    public BattleEntity player;
    public LevelManager levelManager;
    private Animator animator = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    Animator GetAnimator()
    {
        if (!TryGetComponent(out Animator animator))
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).TryGetComponent(out animator))
                {
                    break;
                }
            }
        }
        return animator;
    }

    // Update is called once per frame
    void Update()
    {
        if (animator == null)
        {
            animator = GetAnimator();
        }
        if (entity != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !entity.isBarrier)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = !entity.facingEast;
            }
            Vector3 newPos = entity.position;
            animator?.SetBool("is_moving", !gameObject.transform.localPosition.Equals(newPos));
            animator?.SetBool("is_attacking", entity.isAttacking);
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
        if (otherEntity.isHidden)
        {
            return;
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
