using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioClip backgroundMusicClip;
    public AudioClip[] soundEffectClips;
    [SerializeField] private AudioSource soundEffectSource;
    public static SFXManager instance;

    private AudioSource backgroundMusicSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // create audio source for the music
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true;
        backgroundMusicSource.clip = backgroundMusicClip;
        backgroundMusicSource.volume = 0.1f;
        backgroundMusicSource.Play();
    }

    public void PlaySoundEffect(int index)
    {
        if (index >= 0 && index < soundEffectClips.Length)
        {
            if (soundEffectSource != null && soundEffectClips[index] != null)
            {
                soundEffectSource.PlayOneShot(soundEffectClips[index]);
            }
            else
            {
                Debug.LogWarning("Sound effect source or clip is null.");
            }
        }
        else
        {
            Debug.LogWarning("Index out of range for sound effects array.");
        }
    }
}
