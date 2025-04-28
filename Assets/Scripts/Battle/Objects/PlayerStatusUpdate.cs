using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerStatusUpdate : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    public AudioClip jumpSfxClip;
    public AudioSource jumpSfx;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var player = GetComponentInParent<LevelManager>().player;
        Vector3 newPos = player.position;
        animator.SetBool("is_moving", !transform.localPosition.Equals(newPos));
        animator.SetBool("is_in_air", newPos.y > 0.05);
        animator.SetBool("is_failing", newPos.y - transform.localPosition.y < 0);
        transform.position = newPos;

        if (newPos.y > 0.05)
        {
            if (!jumpSfx.isPlaying)
            {
                jumpSfx.clip = jumpSfxClip;
                jumpSfx.Play(0);
            }
        }
        //else
        //{
        //    jumpSfx.Stop();
        //    jumpSfx.clip = null;
        //}

        GetComponent<SpriteRenderer>().flipX = !player.facingEast;
    }
}
