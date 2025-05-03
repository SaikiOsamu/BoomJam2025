using UnityEngine;
using static ThemeManager;

public class LoopingParallaxLayer : MonoBehaviour
{
    public float scrollSpeed = 0.5f;  // Speed of parallax movement
    public Transform player;
    public SpriteRenderer[] layerParts; // Multiple sprites in each layer
    private float spriteWidth;

    // Theme sprites
    public Sprite forestSprite;
    public Sprite snowLandSprite;
    public Sprite desertSprite;

    private Vector3 lastPlayerPos;

    public ThemeManager.Theme[] supportedThemes; // Set in Inspector or constructor

    void Start()
    {
        if (player != null)
            lastPlayerPos = player.position;

        // Check for missing layer parts
        if (layerParts.Length < 1)
        {
            Debug.LogWarning("Add at least one sprite to make wrapping work!");
            enabled = false;
            return;
        }

        // If layer has a sprite, calculate sprite width
        if (layerParts[0].sprite == null)
        {
            Debug.LogWarning($"{gameObject.name} - Missing sprite in layerParts[0], disabling.");
            enabled = false;
            return;
        }

        spriteWidth = layerParts[0].bounds.size.x;
    }

    void Update()
    {
        if (player == null || layerParts.Length == 0) return;

        Vector3 delta = player.position - lastPlayerPos;
        lastPlayerPos = player.position;

        // Clamp scrollSpeed to [0, 1] to avoid moving faster than player
        float effectiveSpeed = Mathf.Clamp01(scrollSpeed);

        // Move this layer with the player at a scaled speed
        transform.position += Vector3.right * delta.x * effectiveSpeed;

        float camX = Camera.main.transform.position.x;

        // Handle the single sprite case and wrapping
        if (layerParts.Length == 1)
        {
            SpriteRenderer sr = layerParts[0];
            float diff = sr.transform.position.x - camX;

            // Check if the sprite goes off-screen to the left or right
            if (diff + sr.bounds.size.x < -sr.bounds.size.x)
            {
                sr.transform.position += Vector3.right * sr.bounds.size.x;
            }
            else if (diff - sr.bounds.size.x > sr.bounds.size.x)
            {
                sr.transform.position -= Vector3.right * sr.bounds.size.x;
            }
        }
        else
        {
            // Loop through each sprite in the layer and ensure seamless wrapping
            foreach (var sr in layerParts)
            {
                if (sr == null) continue;

                // Calculate the width of the current sprite
                float currentSpriteWidth = sr.bounds.size.x;

                // Calculate the offset needed for seamless wrapping
                float diff = sr.transform.position.x - camX;

                // Wrap left (when sprite goes out of the screen to the left)
                if (diff + currentSpriteWidth < -currentSpriteWidth)
                {
                    sr.transform.position += Vector3.right * currentSpriteWidth * layerParts.Length;
                }

                // Wrap right (when sprite goes out of the screen to the right)
                if (diff - currentSpriteWidth > currentSpriteWidth)
                {
                    sr.transform.position -= Vector3.right * currentSpriteWidth * layerParts.Length;
                }
            }
        }
    }

    public void SwitchTheme(ThemeManager.Theme theme)
    {
        bool themeSupported = System.Array.Exists(supportedThemes, t => t == theme);

        if (!themeSupported)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        Sprite selected = theme == ThemeManager.Theme.Forest ? forestSprite :
                          theme == ThemeManager.Theme.SnowLand ? snowLandSprite :
                          desertSprite;

        bool isActive = selected != null;

        // Switch sprite for all layer parts based on the selected theme
        foreach (var part in layerParts)
        {
            if (part == null) continue;
            part.gameObject.SetActive(isActive);
            if (isActive)
                part.sprite = selected;
        }

        // Update sprite width after switching theme to ensure wrapping logic works correctly
        if (isActive && layerParts.Length > 0 && layerParts[0].sprite != null)
        {
            spriteWidth = layerParts[0].bounds.size.x;  // Update spriteWidth
        }
    }
}
