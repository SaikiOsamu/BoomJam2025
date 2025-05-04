using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeasonManager : MonoBehaviour
{
    public Season currentSeason = Season.Winter;
    public List<StationaryPrefabSpawner> spawners;

    [Header("Background Layer")]
    public SpriteRenderer backgroundRenderer;  // Assign the sprite renderer from BackgroundLayer
    public Sprite winterSprite;
    public Sprite springSprite;
    public float backgroundFadeDuration = 1f;

    private Coroutine fadeCoroutine;

    void Start()
    {
        ApplySeason(currentSeason);
    }

    public void SwitchSeason()
    {
        currentSeason = currentSeason == Season.Winter ? Season.Spring : Season.Winter;
        ApplySeason(currentSeason);
    }

    void ApplySeason(Season newSeason)
    {
        foreach (var spawner in spawners)
        {
            spawner.SwitchSeasonPrefabs(GetPrefabPoolForSpawner(spawner));
        }

        Sprite targetSprite = newSeason == Season.Spring ? springSprite : winterSprite;

        if (backgroundRenderer != null && backgroundRenderer.sprite != targetSprite)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeBackground(targetSprite));
        }
    }

    IEnumerator FadeBackground(Sprite newSprite)
    {
        GameObject fadeObj = new GameObject("FadeRenderer");
        fadeObj.transform.SetParent(backgroundRenderer.transform.parent);
        fadeObj.transform.localPosition = backgroundRenderer.transform.localPosition;

        SpriteRenderer fadeRenderer = fadeObj.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = backgroundRenderer.sprite;
        fadeRenderer.sortingOrder = backgroundRenderer.sortingOrder - 1;
        fadeRenderer.color = new Color(1f, 1f, 1f, 1f);

        backgroundRenderer.sprite = newSprite;
        backgroundRenderer.color = new Color(1f, 1f, 1f, 0f);

        float timer = 0f;

        while (timer < backgroundFadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / backgroundFadeDuration);

            backgroundRenderer.color = new Color(1f, 1f, 1f, t);
            fadeRenderer.color = new Color(1f, 1f, 1f, 1f - t);

            yield return null;
        }

        Destroy(fadeObj);
    }

    private GameObject[] GetPrefabPoolForSpawner(StationaryPrefabSpawner spawner)
    {
        return currentSeason == Season.Spring ? spawner.springPrefabs : spawner.winterPrefabs;
    }
}
