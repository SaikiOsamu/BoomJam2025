using UnityEngine;

public class randomsound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.Moving);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
