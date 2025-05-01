using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuButtonHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text Settings")]
    [SerializeField] private Color normalTextColor = Color.gray;
    [SerializeField] private Color hoverTextColor = Color.white;

    [Header("Background Hover Effect")]
    [SerializeField] private RawImage hoverBackgroundImage;
    [SerializeField] private float backgroundFadeDuration = 0.25f;
    [SerializeField] private AnimationCurve backgroundFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Arrow Settings")]
    [SerializeField] private RawImage leftArrow;
    [SerializeField] private RawImage rightArrow;
    [SerializeField] private float arrowFadeDuration = 0.2f;
    [SerializeField] private AnimationCurve arrowFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float arrowHoverDistance = 10f;

    // References
    private TextMeshProUGUI menuText;
    private Coroutine backgroundTransition;
    private Coroutine arrowsTransition;

    // Original positions
    private Vector3 leftArrowOriginalPos;
    private Vector3 rightArrowOriginalPos;

    private void Awake()
    {
        // Get the TextMeshProUGUI component (either on this object or child)
        menuText = GetComponent<TextMeshProUGUI>();
        // If not found on this GameObject, try to find it in children
        if (menuText == null)
        {
            menuText = GetComponentInChildren<TextMeshProUGUI>();
            if (menuText == null)
            {
                Debug.LogError("MenuButtonHoverController requires a TextMeshProUGUI component (either on this object or a child)!");
                return;
            }
        }

        // We no longer need to store arrow positions since we won't modify them
        // Just set initial state
        SetHoverState(false, true);
    }

    // Called when pointer enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHoverState(true);
    }

    // Called when pointer exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHoverState(false);
    }

    // Set the hover state for all elements
    private void SetHoverState(bool isHovered, bool instant = false)
    {
        // Change text color
        if (menuText != null)
        {
            menuText.color = isHovered ? hoverTextColor : normalTextColor;
        }

        // Handle background transition
        if (hoverBackgroundImage != null)
        {
            // Stop any ongoing transition
            if (backgroundTransition != null)
            {
                StopCoroutine(backgroundTransition);
            }

            // Start transition
            if (instant)
            {
                Color bgColor = hoverBackgroundImage.color;
                bgColor.a = isHovered ? 1f : 0f;
                hoverBackgroundImage.color = bgColor;
            }
            else
            {
                backgroundTransition = StartCoroutine(TransitionBackground(isHovered));
            }
        }

        // Handle arrows transition (alpha only)
        if ((leftArrow != null || rightArrow != null) && !instant)
        {
            // Stop any ongoing transition
            if (arrowsTransition != null)
            {
                StopCoroutine(arrowsTransition);
            }

            // Start transition
            arrowsTransition = StartCoroutine(TransitionArrows(isHovered));
        }
        else if ((leftArrow != null || rightArrow != null) && instant)
        {
            // Set arrow alpha instantly
            SetArrowsAlpha(isHovered ? 1f : 0f);

            // No position changes - arrows stay where they are
        }
    }

    // Smoothly transition the background alpha
    private IEnumerator TransitionBackground(bool fadeIn)
    {
        float startAlpha = hoverBackgroundImage.color.a;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < backgroundFadeDuration)
        {
            // Use unscaledDeltaTime instead of deltaTime
            elapsedTime += Time.unscaledDeltaTime;
            float t = backgroundFadeCurve.Evaluate(elapsedTime / backgroundFadeDuration);

            Color newColor = hoverBackgroundImage.color;
            newColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            hoverBackgroundImage.color = newColor;

            yield return null;
        }

        // Ensure we end at correct alpha
        Color finalColor = hoverBackgroundImage.color;
        finalColor.a = targetAlpha;
        hoverBackgroundImage.color = finalColor;

        backgroundTransition = null;
    }

    // Smoothly transition the arrows (only alpha, no position change)
    private IEnumerator TransitionArrows(bool fadeIn)
    {
        float startAlpha = leftArrow != null ? leftArrow.color.a : (rightArrow != null ? rightArrow.color.a : 0f);
        float targetAlpha = fadeIn ? 1f : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < arrowFadeDuration)
        {
            // Use unscaledDeltaTime instead of deltaTime
            elapsedTime += Time.unscaledDeltaTime;
            float t = arrowFadeCurve.Evaluate(elapsedTime / arrowFadeDuration);

            // Update alpha only, preserve positions
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetArrowsAlpha(currentAlpha);

            yield return null;
        }

        // Ensure we end at correct alpha
        SetArrowsAlpha(targetAlpha);
        arrowsTransition = null;
    }

    // Helper to set alpha on both arrows
    private void SetArrowsAlpha(float alpha)
    {
        if (leftArrow != null)
        {
            Color leftColor = leftArrow.color;
            leftColor.a = alpha;
            leftArrow.color = leftColor;
        }

        if (rightArrow != null)
        {
            Color rightColor = rightArrow.color;
            rightColor.a = alpha;
            rightArrow.color = rightColor;
        }
    }
}