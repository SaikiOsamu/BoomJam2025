using UnityEngine;
using System.Collections.Generic;

public enum Season { Winter, Spring }

public class StationaryPrefabSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject[] winterPrefabs;
    public GameObject[] springPrefabs;

    [HideInInspector]
    public GameObject[] spawnPrefabs; // Currently active pool (assigned by SeasonSwitcher)

    public float spawnDistance = 5f;
    public float spawnXOffset = 12f;
    public float yOffset = 0f;
    public float distanceMultiplier = 1f;
    public float destroyOffset = 15f;

    private float lastSpawnX;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        if (player != null)
            lastSpawnX = player.position.x;

        // Default to winter at start
        spawnPrefabs = springPrefabs;

        for (int i = 1; i <= 2; i++)
        {
            Vector3 direction = Vector3.right;
            SpawnObject(direction, offsetMultiplier: i);
        }
    }

    void Update()
    {
        if (player == null || spawnPrefabs.Length == 0) return;

        float distanceMoved = Mathf.Abs(player.position.x - lastSpawnX);
        if (distanceMoved >= spawnDistance * distanceMultiplier)
        {
            Vector3 direction = player.position.x > lastSpawnX ? Vector3.right : Vector3.left;
            lastSpawnX = player.position.x;
            SpawnObject(direction);
        }

        // Cleanup
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            if (obj == null || Mathf.Abs(obj.transform.position.x - player.position.x) > destroyOffset)
            {
                Destroy(obj);
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    public void SetSeason(Season season)
    {
        spawnPrefabs = season == Season.Spring ? springPrefabs : winterPrefabs;

        // Destroy all existing spawned objects
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedObjects.Clear();

        // Optional: spawn some immediately
        for (int i = 1; i <= 2; i++)
        {
            SpawnObject(Vector3.right, offsetMultiplier: i);
        }

        Debug.Log($"[{gameObject.name}] Switched to {season} season prefabs.");
    }


    void SpawnObject(Vector3 direction, float offsetMultiplier = 1f)
    {
        if (spawnPrefabs.Length == 0) return;

        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];

        // Get bounds to align on ground
        GameObject temp = Instantiate(prefab);
        SpriteRenderer sr = temp.GetComponentInChildren<SpriteRenderer>();
        float prefabHeight = sr != null ? sr.bounds.size.y : 0f;
        Destroy(temp);

        float alignedY = transform.position.y + yOffset + (prefabHeight / 2f);

        Vector3 spawnPos = new Vector3(
            player.position.x + direction.x * spawnXOffset * offsetMultiplier,
            alignedY,
            transform.position.z
        );

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        spawnedObjects.Add(instance);
    }

    public void SwitchSeasonPrefabs(GameObject[] newPrefabs)
    {
        // Fade out existing prefabs
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                FadeAndDestroy fadeScript = obj.GetComponent<FadeAndDestroy>();
                if (fadeScript != null)
                    fadeScript.StartFade();
                else
                    Destroy(obj); // fallback if no fade script
            }
        }

        spawnedObjects.Clear();

        // Assign the new season pool
        spawnPrefabs = newPrefabs;

        // Optionally spawn a few new prefabs
        for (int i = 1; i <= 2; i++)
        {
            SpawnObject(Vector3.right, offsetMultiplier: i);
        }

        Debug.Log($"[{gameObject.name}] Switched season with fade-out.");
    }


}
