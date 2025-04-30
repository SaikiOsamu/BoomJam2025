using System.Linq;
using UnityEngine;

public static class AnimatorExtensions
{

    public static void TrySetParam(this Animator animator, string paramName, bool paramValue)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
            {
                animator.SetBool(paramName, paramValue);
                return;
            }
        }
    }
}

public class ObjectStatusUpdate : MonoBehaviour
{

    public BattleEntity entity;
    public BattleEntity player;
    public LevelManager levelManager;
    private Animator animator = null;
    public AudioSource audioSource = null;
    public bool spawnSoundPlayed = false;
    public bool isDying = false;

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
        if (isDying)
        {
            if (!audioSource.isPlaying)
            {
                Destroy(gameObject);
            }
            return;
        }
        if (animator == null)
        {
            animator = GetAnimator();
        }
        if (entity != null)
        {
            if (!spawnSoundPlayed)
            {
                spawnSoundPlayed = true;
                AudioClip spawnSound = entity.prefabCharacter?.onSpawn;
                if (spawnSound != null)
                {
                    audioSource.clip = spawnSound;
                    audioSource.Play();
                }
            }
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !entity.isBarrier)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = !entity.facingEast;
            }
            Vector3 newPos = entity.position;
            animator?.TrySetParam("is_moving", !gameObject.transform.localPosition.Equals(newPos));
            animator?.TrySetParam("is_attacking", entity.isAttacking);
            animator?.TrySetParam("is_slowed", levelManager.timeExtender != null);
            gameObject.transform.localPosition = entity.position;
            gameObject.transform.localRotation = entity.rotation;
            if (!entity.isAlive)
            {
                AudioClip despawnSound = entity.prefabCharacter?.onDespawn;
                if (despawnSound != null)
                {
                    audioSource.clip = despawnSound;
                    audioSource.Play();
                }
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
                isDying = true;
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
