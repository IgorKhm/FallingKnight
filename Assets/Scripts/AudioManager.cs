using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip musicLoop;
    public AudioClip uiClick;
    public AudioClip playerHit;
    public AudioClip objectImpact;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic()
    {
        if (musicSource == null || musicLoop == null) return;
        if (musicSource.clip != musicLoop) musicSource.clip = musicLoop;
        if (!musicSource.isPlaying) musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // Convenience wrappers
    public void PlayUIClick() => PlaySfx(uiClick);
    public void PlayPlayerHit() => PlaySfx(playerHit);
    public void PlayObjectImpact() => PlaySfx(objectImpact);
}
