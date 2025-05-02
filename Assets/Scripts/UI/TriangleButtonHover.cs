using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TriangleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Texture Settings")]
    [SerializeField] private Texture2D normalTexture;     // The default triangle texture
    [SerializeField] private Texture2D hoverTexture;      // The glowing triangle texture

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // References
    private RawImage buttonImage;
    private Coroutine currentTransition;

    private void Awake()
    {
        // Get the RawImage component
        buttonImage = GetComponent<RawImage>();

        // Add RawImage component if it doesn't exist
        if (buttonImage == null)
        {
            buttonImage = gameObject.AddComponent<RawImage>();
        }

        // Set the default texture
        if (normalTexture != null)
        {
            buttonImage.texture = normalTexture;
        }
    }

    // Called when pointer enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any ongoing transition
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // Start transition to hover texture
        if (hoverTexture != null)
        {
            currentTransition = StartCoroutine(TransitionToTexture(hoverTexture));
        }
    }

    // Called when pointer exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop any ongoing transition
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // Start transition to normal texture
        if (normalTexture != null)
        {
            currentTransition = StartCoroutine(TransitionToTexture(normalTexture));
        }
    }

    // Smoothly transition between textures
    private IEnumerator TransitionToTexture(Texture2D targetTexture)
    {
        buttonImage.texture = targetTexture;

        // Create a temporary material for the transition effect
        Material transitionMaterial = new Material(buttonImage.material);
        buttonImage.material = transitionMaterial;

        // Start with fully transparent
        Color startColor = buttonImage.color;
        startColor.a = 0f;
        buttonImage.color = startColor;

        // Animate to fully opaque
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            // Use unscaledDeltaTime instead of deltaTime so it works when game is paused
            elapsedTime += Time.unscaledDeltaTime;
            float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            Color newColor = buttonImage.color;
            newColor.a = Mathf.Lerp(0f, 1f, t);
            buttonImage.color = newColor;

            yield return null;
        }

        // Ensure we end at fully opaque
        Color endColor = buttonImage.color;
        endColor.a = 1f;
        buttonImage.color = endColor;

        // Reset to default material
        buttonImage.material = null;

        currentTransition = null;
    }
}