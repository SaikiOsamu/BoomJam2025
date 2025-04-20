using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    private Light lightToFlicker;
    [SerializeField, Range(0f, 13105f)] private float minIntensity = 0f;
    [SerializeField, Range(0f, 13105f)] private float maxIntensity = 13105f;
    [SerializeField, Min(0f)] private float timeBetweenIntensity = 0.04f;

    private float currentTimer;

    private void Awake()
    {
        if (lightToFlicker == null)
        {
            lightToFlicker = GetComponent<Light>();
        }

        ValidateIntensityBounds();
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;
        if (currentTimer < timeBetweenIntensity) return;
        lightToFlicker.intensity = Random.Range(minIntensity, maxIntensity);
        currentTimer = 0;
    }

    private void ValidateIntensityBounds()
    {
        if (minIntensity > maxIntensity)
        {
            Debug.LogWarning("Min Intensity is greater than max Intensity, swapping values!");
            (minIntensity, maxIntensity) = (maxIntensity, minIntensity);
        }
    }
}
