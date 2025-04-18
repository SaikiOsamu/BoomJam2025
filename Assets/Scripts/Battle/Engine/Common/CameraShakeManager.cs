using UnityEngine;

[DisallowMultipleComponent]
public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField]
    private ShakeableTransform shakeable;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float stress)
    {
        shakeable.InduceStress(stress);
    }
}
