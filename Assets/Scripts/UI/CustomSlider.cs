using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CustomSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Slider Components")]
    [SerializeField] private RawImage sliderBar;            // The rectangle bar image
    [SerializeField] private RawImage sliderHandle;         // The cylinder handle image
    [SerializeField] private RectTransform handleRect;      // Reference to the handle's RectTransform

    [Header("Slider Settings")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private float currentValue = 0.5f;
    [SerializeField] private bool wholeNumbers = false;

    [Header("Visual Settings")]
    [SerializeField] private Color normalHandleColor = Color.white;
    [SerializeField] private Color hoverHandleColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color dragHandleColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    // References
    private RectTransform sliderBarRect;
    private bool isDragging = false;

    // Event to notify value changes
    public event Action<float> OnValueChanged;

    private void Awake()
    {
        // Get references
        if (handleRect == null && sliderHandle != null)
        {
            handleRect = sliderHandle.GetComponent<RectTransform>();
        }

        if (sliderBarRect == null && sliderBar != null)
        {
            sliderBarRect = sliderBar.GetComponent<RectTransform>();
        }

        // Set initial position
        UpdateHandlePosition();
    }

    // Public getter and setter for the slider value
    public float Value
    {
        get { return currentValue; }
        set
        {
            SetValue(value);
        }
    }

    // Set the value and update visuals
    public void SetValue(float value, bool sendCallback = true)
    {
        // Clamp the value
        value = Mathf.Clamp(value, minValue, maxValue);

        // Make it a whole number if that setting is enabled
        if (wholeNumbers)
        {
            value = Mathf.Round(value);
        }

        // Only update if the value has changed
        if (currentValue != value)
        {
            currentValue = value;
            UpdateHandlePosition();

            // Invoke the callback
            if (sendCallback && OnValueChanged != null)
            {
                OnValueChanged.Invoke(currentValue);
            }
        }
    }

    // Update the handle position based on current value
    private void UpdateHandlePosition()
    {
        if (handleRect == null || sliderBarRect == null) return;

        // Calculate normalized value between 0-1
        float normalizedValue = (currentValue - minValue) / (maxValue - minValue);

        // Calculate x position within the slider bar
        float handleX = sliderBarRect.rect.width * normalizedValue;

        // Set the position
        Vector2 anchoredPosition = handleRect.anchoredPosition;
        anchoredPosition.x = handleX - (sliderBarRect.rect.width * 0.5f);
        handleRect.anchoredPosition = anchoredPosition;
    }

    // Calculate the value based on a position within the slider bar
    private float CalculateValueFromPosition(Vector2 position)
    {
        // Convert screen position to local position within the slider bar
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderBarRect, position, null, out Vector2 localPoint);

        // Calculate normalized position (0-1) along the bar width
        float normalizedPosition = (localPoint.x + sliderBarRect.rect.width * 0.5f) / sliderBarRect.rect.width;
        normalizedPosition = Mathf.Clamp01(normalizedPosition);

        // Convert to actual value
        float newValue = minValue + normalizedPosition * (maxValue - minValue);

        // Make it a whole number if that setting is enabled
        if (wholeNumbers)
        {
            newValue = Mathf.Round(newValue);
        }

        return newValue;
    }

    // Handle pointer events
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        // Change handle color
        if (sliderHandle != null)
        {
            sliderHandle.color = dragHandleColor;
        }

        // Set value based on click position
        SetValue(CalculateValueFromPosition(eventData.position));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            SetValue(CalculateValueFromPosition(eventData.position));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // Restore handle color
        if (sliderHandle != null)
        {
            sliderHandle.color = normalHandleColor;
        }
    }

    // Optional hover effect for the handle
    private void OnPointerEnter()
    {
        if (!isDragging && sliderHandle != null)
        {
            sliderHandle.color = hoverHandleColor;
        }
    }

    private void OnPointerExit()
    {
        if (!isDragging && sliderHandle != null)
        {
            sliderHandle.color = normalHandleColor;
        }
    }
}