using UnityEngine;

public class ShakeTester : MonoBehaviour
{
    private ShakeableTransform shakeScript;

    void Start()
    {
        shakeScript = GetComponent<ShakeableTransform>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)&&(shakeScript != null))
        {
            shakeScript.InduceStress(0.8f); // larger values will result in more shake
        }
    }
}