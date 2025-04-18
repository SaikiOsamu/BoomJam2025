using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float scrollSpeed = 1f;          // Speed of this layer
    public GameObject[] spawnPrefabs;       // Sprites to spawn
    public float spawnInterval = 2f;        // How often to spawn
    public Vector2 spawnYRange = new Vector2(-1f, 1f); // Vertical spawn range
    public float offset = 12f;

    private float timer;

    void Update()
    {
        // Move the whole layer left
        transform.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;

        // Handle spawning
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        if (spawnPrefabs.Length == 0) return;

        // Choose random prefab and spawn position
        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
        //float yOffset = Random.Range(spawnYRange.x, spawnYRange.y);
        Vector3 spawnPos = new Vector3(Camera.main.transform.position.x + offset, transform.position.y, transform.position.z);

        GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        spawned.AddComponent<LayerMover>().speed = scrollSpeed;
    }
}
