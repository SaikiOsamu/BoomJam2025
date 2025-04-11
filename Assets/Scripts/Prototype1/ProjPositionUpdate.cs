using UnityEngine;

public class ProjPositionUpdate : MonoBehaviour
{
    public Projection proj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (proj != null)
        {
            gameObject.transform.localPosition = proj.position;
            if (!proj.is_alive)
            {
                Destroy(gameObject);
            }
        }
    }
}
