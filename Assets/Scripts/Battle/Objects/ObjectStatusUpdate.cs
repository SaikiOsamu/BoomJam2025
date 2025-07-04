using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

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
    public static List<T> GetEachComponent<T>(this GameObject gameObject)
    {
        List<T> effects = new List<T>();
        if (gameObject.TryGetComponent(out T vfx))
        {
            effects.Add(vfx);
        }
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            effects.AddRange(gameObject.transform.GetChild(i).gameObject.GetEachComponent<T>());
        }
        return effects;
    }
}

public class ObjectStatusUpdate : MonoBehaviour
{

    public BattleEntity entity;
    public BattleEntity player;
    public LevelManager levelManager;
    private Animator animator = null;
    private List<VisualEffect> vfx = null;
    private List<ParticleSystem> particles = null;
    private List<SpriteRenderer> renderers = null;
    public AudioSource audioSource = null;
    public bool spawnSoundPlayed = false;
    public bool isDying = false;
    public bool isSlowed = false;
    public float deathCounter = 0;
    public float afterlifeTime = 0;

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
            deathCounter += Time.deltaTime * (isSlowed ? 0.2f : 1);
            if (!audioSource.isPlaying && deathCounter > afterlifeTime)
            {
                Destroy(gameObject);
            }
            return;
        }
        if (animator == null)
        {
            animator = GetAnimator();
        }
        if (vfx == null)
        {
            vfx = gameObject.GetEachComponent<VisualEffect>();
        }
        if (particles == null)
        {
            particles = gameObject.GetEachComponent<ParticleSystem>();
        }
        if (renderers == null)
        {
            renderers = gameObject.GetEachComponent<SpriteRenderer>();
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
            Vector3 newPos = entity.position;
            animator?.TrySetParam("is_ultimate", entity.isUltimate);
            if (entity.isUltimate)
            {
                animator?.TrySetParam("is_moving", false);
                animator?.TrySetParam("is_attacking", false);
                animator?.TrySetParam("is_casting", false);
            }
            else
            {
                animator?.TrySetParam("is_attacking", entity.isAttacking);
                animator?.TrySetParam("is_casting", entity.isCasting);
                if (entity.isAttacking || entity.isCasting)
                {
                    animator?.TrySetParam("is_moving", false);
                }
                else
                {
                    animator?.TrySetParam("is_moving", !gameObject.transform.localPosition.Equals(newPos));
                }
            }
            gameObject.transform.localPosition = entity.position + new Vector2(0, -3);
            gameObject.transform.localRotation = entity.rotation;
            if (!entity.isBarrier && !entity.isBoss)
            {
                foreach (var renderer in renderers)
                {
                    renderer.flipX = !entity.facingEast;
                }
            }
            else if (entity.isBoss)
            {
                if (entity.facingEast)
                {
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    transform.rotation = Quaternion.AngleAxis(180, new Vector3(0, 1, 0));
                }
            }
            if (!entity.isAlive)
            {
                AudioClip despawnSound = entity.prefabCharacter?.onDespawn;
                if (despawnSound != null)
                {
                    audioSource.clip = despawnSound;
                    audioSource.Play();
                }
                foreach (var r in renderers)
                {
                    r.enabled = false;
                }
                isDying = true;
                if (TryGetComponent(out MeshRenderer renderer))
                {
                    renderer.enabled = false;
                }
                foreach (var p in particles)
                {
                    var emission = p.emission;
                    emission.enabled = false;
                    var main = p.main;
                    afterlifeTime = Mathf.Max(afterlifeTime, main.startLifetime.constant);
                }
            }
        }
        // Maybe slow or unslow the particle.
        if (!isSlowed && levelManager.timeExtender != null)
        {
            foreach (var v in vfx)
            {
                v.playRate = 0.2f;
            }
            foreach (var p in particles)
            {
                var main = p.main;
                main.simulationSpeed = 0.2f;
            }
        }
        if (isSlowed && levelManager.timeExtender == null)
        {
            foreach (var v in vfx)
            {
                v.playRate = 1;
            }
            foreach (var p in particles)
            {
                var main = p.main;
                main.simulationSpeed = 1;
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
        if (otherEntity.isHidden)
        {
            return;
        }
        levelManager.RegisterCollision(entity, otherEntity);
    }
}
