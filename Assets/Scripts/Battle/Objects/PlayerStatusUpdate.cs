using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerStatusUpdate : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private LevelManager levelManager;
    public AudioClip jumpSfxClip;
    public AudioSource jumpSfx;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var player = levelManager.player;
        Vector3 newPos = player.position;
        animator.SetBool("is_moving", !transform.localPosition.Equals(newPos));
        animator.SetBool("is_in_air", newPos.y > 0.05);
        animator.SetBool("is_failing", newPos.y - transform.localPosition.y < 0);
        transform.position = newPos + new Vector3(0, -3, 0);

        if (jumpSfxClip != null && newPos.y > 0.05)
        {
            if (!jumpSfx.isPlaying)
            {
                jumpSfx.clip = jumpSfxClip;
                jumpSfx.Play(0);
            }
        }

        GetComponent<SpriteRenderer>().flipX = !player.facingEast;

        if (levelManager.timeExtender != null)
        {
            gameObject.layer = 6;
        }
        else
        {
            gameObject.layer = 0;
        }
    }
}
