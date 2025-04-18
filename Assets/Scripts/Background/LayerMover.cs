using UnityEngine;

public class LayerMover : MonoBehaviour
{
    public float speed = 1f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < Camera.main.transform.position.x - 20f)
        {
            Destroy(gameObject);
        }
    }

}
