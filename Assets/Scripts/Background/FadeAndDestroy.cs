using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
{
    public float fadeDuration = 1f;
    private SpriteRenderer[] spriteRenderers;
    private float timer = 0f;
    private bool fading = false;

    void Start()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void StartFade()
    {
        if (!fading)
        {
            fading = true;
            timer = 0f;
        }
    }

    void Update()
    {
        if (!fading) return;

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}

