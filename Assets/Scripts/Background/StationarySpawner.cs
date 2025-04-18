using UnityEngine;
using System.Collections.Generic;

public class StationaryPrefabSpawner : MonoBehaviour
{
    public Transform player;                // Reference to player
    public GameObject[] spawnPrefabs;       // Prefabs to spawn
    public float spawnDistance = 5f;        // Player movement before next spawn
    public float spawnXOffset = 12f;        // Spawn just offscreen to the right
    public float yOffset = 0f;              // Vertical offset
    public float distanceMultiplier = 1f;   // Controls how often to spawn
    public float destroyOffset = 15f;       // How far behind player before destroying

    private float lastSpawnX;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        if (player != null)
            lastSpawnX = player.position.x;

        Debug.Log($"[{gameObject.name}] StationaryPrefabSpawner initialized. Waiting for player to move {spawnDistance * distanceMultiplier} units.");

        for (int i = 1; i <= 2; i++)
        {
            Vector3 direction = Vector3.right;
            SpawnObject(direction, offsetMultiplier: i);
        }

        Debug.Log($"[{gameObject.name}] StationaryPrefabSpawner initialized. Waiting for player to move {spawnDistance * distanceMultiplier} units.");
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No player assigned!");
            return;
        }

        if (spawnPrefabs.Length == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No prefabs assigned to spawn.");
            return;
        }

        float distanceMoved = Mathf.Abs(player.position.x - lastSpawnX);
        if (distanceMoved >= spawnDistance * distanceMultiplier)
        {
            Vector3 direction = player.position.x > lastSpawnX ? Vector3.right : Vector3.left;

            lastSpawnX = player.position.x;
            SpawnObject(direction);
        }

        // Destroy prefabs that fall behind player
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            if (obj == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            if (Mathf.Abs(obj.transform.position.x - player.position.x) > destroyOffset)
            {
                Debug.Log($"[{gameObject.name}] Destroying '{obj.name}' at {obj.transform.position}");
                Destroy(obj);
                spawnedObjects.RemoveAt(i);
            }
        }
    }


    void SpawnObject(Vector3 direction, float offsetMultiplier = 1f)
    {
        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];

        // Temporary instance to measure bounds without showing it
        GameObject temp = Instantiate(prefab);
        SpriteRenderer sr = temp.GetComponentInChildren<SpriteRenderer>();

        float prefabHeight = sr != null ? sr.bounds.size.y : 0f;
        Destroy(temp);

        // Align bottom to ground
        float alignedY = transform.position.y + yOffset + (prefabHeight / 2f);

        Vector3 spawnPos = new Vector3(
            player.position.x + direction.x * spawnXOffset * offsetMultiplier,
            alignedY,
            transform.position.z
        );

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        spawnedObjects.Add(instance);

        Debug.Log($"[{gameObject.name}] Spawned prefab '{prefab.name}' at {spawnPos} (aligned to ground)");
    }


}
