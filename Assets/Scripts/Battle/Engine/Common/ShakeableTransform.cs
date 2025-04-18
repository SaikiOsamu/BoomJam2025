using UnityEngine;

public class ShakeableTransform:MonoBehaviour
{
    // Maximum translation shake distance in each direction
    [SerializeField]
    Vector3 maxTranslationShake = Vector3.one * 1;
    // Maximum rotation angle during shaking
    [SerializeField]
    Vector3 maxAngularShake = Vector3.one * 1;
    // Frequency of the Perlin noise function. Higher values will result in faster shaking.
    [SerializeField]
    float frequency = 25;
    // Higher values will result in a smoother falloff as trauma reduces.
    [SerializeField]
    float traumaExponent = 2;
    // Amount of trauma per second that is recovered. Higher values will result in faster recovery.
    [SerializeField]
    float recoverySpeed = 1;
    private float trauma = 0;
    private float seed;
    private void Awake()
    {
        seed = Random.value;
    }
    private void Update()
    {
        float shake = Mathf.Pow(trauma, traumaExponent); // smoother falloff
        transform.localPosition = new Vector3(
            maxTranslationShake.x * (Mathf.PerlinNoise(seed, Time.time * frequency) * 2 - 1), 
            maxTranslationShake.y * (Mathf.PerlinNoise(seed+1, Time.time * frequency) * 2 - 1),
            maxTranslationShake.z * (Mathf.PerlinNoise(seed+2, Time.time * frequency) * 2 - 1)
        )*shake;
        transform.localRotation = Quaternion.Euler(new Vector3(
            maxAngularShake.x * (Mathf.PerlinNoise(seed+3, Time.time * frequency) * 2 - 1), 
            maxAngularShake.y * (Mathf.PerlinNoise(seed+4, Time.time * frequency) * 2 - 1),
            maxAngularShake.z * (Mathf.PerlinNoise(seed+5, Time.time * frequency) * 2 - 1)
        )*shake);
        trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
    }
    public void InduceStress(float stress)
    {
        trauma = Mathf.Clamp01(trauma + stress);
    }
}