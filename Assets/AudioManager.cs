using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource SFXSource;

    [Header("Audio Clips")]
    public AudioClip Spring;
    public AudioClip Summer;
    public AudioClip Fall;
    public AudioClip Winter;
    public AudioClip BossFight;
    public AudioClip Monster_moving_SFX;
    public AudioClip Moving;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AudioManager instance;
    private void Awake()
    {
        if(instance ==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        PlayMusic(Spring);
    }
    public void PlayMusic(AudioClip musicClip)
    {
        if(musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    public void PlaySFX(AudioClip sfxClip)
    {
        if(SFXSource != null && sfxClip != null)
        {
            SFXSource.PlayOneShot(sfxClip);
        }
    }
    public void StopMusic()
    {
        if(musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
