using UnityEngine;

public class TowerPositionUpdate : MonoBehaviour
{
    public Tower tower;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (tower != null)
        {
            gameObject.transform.localPosition = tower.position;
        }
    }
}
