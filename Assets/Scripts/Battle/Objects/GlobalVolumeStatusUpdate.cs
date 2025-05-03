using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GlobalVolumeStatusUpdate : MonoBehaviour
{
    [SerializeField]
    private LevelManager levelManager;
    [SerializeField]
    private Volume volume;
    private float timeExtendingStatus = 0;
    [SerializeField]
    private float speed = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            if (colorAdjustments.saturation.value > -100 && levelManager.timeExtender != null)
            {
                colorAdjustments.saturation.value -= speed * Time.deltaTime;
            }
            else if (colorAdjustments.saturation.value < 0 && levelManager.timeExtender == null)
            {
                colorAdjustments.saturation.value += speed * Time.deltaTime;
            }
        }
    }
}
