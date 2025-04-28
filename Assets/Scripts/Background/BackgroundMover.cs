using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    public Transform player;
    private Vector3 lastPlayerPos;

    void Start()
    {
        if (player != null)
            lastPlayerPos = player.position;
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 delta = player.position - lastPlayerPos;
            transform.position += new Vector3(delta.x, 0f, 0f); // Only horizontal follow
            lastPlayerPos = player.position;
        }
    }
}


