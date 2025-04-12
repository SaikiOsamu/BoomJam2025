using UnityEngine;

public class MoveRotate : MonoBehaviour
{
    [SerializeField]
    public float speed = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.localRotation = Quaternion.AngleAxis(
            gameObject.GetComponentInParent<LevelManager>().player.position.x * speed, new Vector3(0, 0, 1));
    }
}
