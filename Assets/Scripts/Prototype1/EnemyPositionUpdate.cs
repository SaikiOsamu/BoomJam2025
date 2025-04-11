using UnityEngine;

public class EnemyPositionUpdate : MonoBehaviour
{
    public Enemy enemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (enemy != null)
        {
            gameObject.transform.localPosition = enemy.position;
            if (!enemy.is_alive)
            {
                Destroy(gameObject);
            }
        }
    }
}
