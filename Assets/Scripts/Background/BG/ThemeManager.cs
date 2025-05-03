using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class ThemeManager : MonoBehaviour
{
    public enum Theme
    {
        Forest,
        SnowLand,
        Desert
    }

    public Theme currentTheme = Theme.Forest;
    public List<LoopingParallaxLayer> parallaxLayers;

    [Header("Background Layer")]
    public SpriteRenderer backgroundRenderer;
    public Sprite forestSprite;
    public Sprite snowLandSprite;
    public Sprite desertSprite;
    public float backgroundFadeDuration = 1f;
    private Coroutine fadeCoroutine;

    [Header("Background Movement")]
    public Transform player;        // Assign your player Transform
    public float backgroundScrollSpeed = 0.2f;

    private Vector3 lastPlayerPos;



    void Start()
    {
        if (player != null)
            lastPlayerPos = player.position;

        ApplyTheme(currentTheme);
    }

    void Update()
    {
        if (player == null || backgroundRenderer == null) return;

        Vector3 delta = player.position - lastPlayerPos;
        lastPlayerPos = player.position;

        // Move background with slower speed for parallax effect
        backgroundRenderer.transform.position += new Vector3(delta.x * backgroundScrollSpeed, 0f, 0f);
    }

    public void SwitchTheme()
    {
        currentTheme = currentTheme == Theme.Forest ? Theme.SnowLand :
        currentTheme == Theme.SnowLand ? Theme.Desert :
                        Theme.Forest;
        ApplyTheme(currentTheme);
    }

    void ApplyTheme(Theme newTheme)
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer == null)
            {
                Debug.LogWarning("ThemeManager: One of the parallaxLayers is null.");
                continue;
            }

            try
            {
                layer.SwitchTheme(newTheme);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error switching theme on layer {layer.name}: {ex.Message}");
            }
        }

        Sprite targetSprite = newTheme == Theme.Forest ? forestSprite :
                              newTheme == Theme.SnowLand ? snowLandSprite :
                              desertSprite;

        if (backgroundRenderer != null && targetSprite != null && backgroundRenderer.sprite != targetSprite)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeBackground(targetSprite));
        }
        else if (targetSprite == null)
        {
            Debug.LogWarning("ThemeManager: Target background sprite is missing for this theme.");
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
}
