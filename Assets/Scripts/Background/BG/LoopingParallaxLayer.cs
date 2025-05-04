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

    

    [Header("Follow Player (Optional)")]
    public float followSpeed = 0.2f;     // Parallax scroll speed
    public float offset = 0f;            // Offset between layer and player
    private Vector3 lastPlayerPos;

    public ThemeManager.Theme[] supportedThemes; // Set in Inspector or constructor

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized) return;

        // Do any one-time setup here (if needed)
        // For example: calculating spriteWidth if a valid sprite is present
        if (layerParts.Length > 0 && layerParts[0] != null && layerParts[0].sprite != null)
        {
            spriteWidth = layerParts[0].bounds.size.x;
        }

        initialized = true;
    }



    void Start()
    {
        if (player != null)
            lastPlayerPos = player.position;

        if (gameObject.activeInHierarchy)
        {
            Initialize();
        }
    }


    void Update()
    {
        if (player == null || layerParts.Length == 0) return;

        // Apply parallax movement to layers with an offset
        float targetX = (player.position.x * followSpeed) + offset; // Adjust with offset
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        WrapSprites();
    }


    void WrapSprites()
    {
        float camX = Camera.main.transform.position.x;

        foreach (var sr in layerParts)
        {
            if (sr == null) continue;

            float currentWidth = sr.bounds.size.x;
            float diff = sr.transform.position.x - camX;

            if (diff + currentWidth < -currentWidth)
            {
                sr.transform.position += Vector3.right * currentWidth * layerParts.Length;
            }
            else if (diff - currentWidth > currentWidth)
            {
                sr.transform.position -= Vector3.right * currentWidth * layerParts.Length;
            }
        }
    }


    public void SwitchTheme(ThemeManager.Theme theme)
    {
        bool themeSupported = System.Array.Exists(supportedThemes, t => t == theme);

        if (!themeSupported)
        {
            // Unsupported theme: hide visuals and skip logic
            foreach (var part in layerParts)
            {
                if (part != null)
                    part.enabled = false;
            }

            // Still keep script enabled to allow theme switching later
            return;
        }

        // Supported theme
        Sprite selected = theme == ThemeManager.Theme.Forest ? forestSprite :
                          theme == ThemeManager.Theme.SnowLand ? snowLandSprite :
                          desertSprite;

        bool isActive = selected != null;

        foreach (var part in layerParts)
        {
            if (part == null) continue;
            part.enabled = isActive;
            part.sprite = isActive ? selected : null;
        }

        if (isActive && layerParts.Length > 0 && layerParts[0].sprite != null)
        {
            spriteWidth = layerParts[0].bounds.size.x;
        }
    }

}
