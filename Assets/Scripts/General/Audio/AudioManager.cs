using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource soundFXPrefab;
    private List<AudioSource> loopSources;
    private List<AudioSource> audioPool = new List<AudioSource>();
    [SerializeField] int maxSoundsPlaying = 25;
    private int currentSoundsPlaying = 0;

    [Header("UI Sound")]
    public AudioClip buttonSound;
    public AudioClip hoverSound;

    private void Start()
    {
        InitializeAudioPool();
    }

    public void PlayPooledSound(AudioClip clip, float volume = 1f, float pitch = 1f) // for sounds that should be played instantly (basically player actions)
    {
        AudioSource audioSource = GetPooledAudioSource();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();

        StartCoroutine(DeactivateAfterPlay(audioSource));
    }
    private void InitializeAudioPool()
    {
        for (int i = 0; i < 15; i++)  // Pool size of 15
        {
            AudioSource audioSource = Instantiate(soundFXPrefab, transform);
            audioSource.gameObject.SetActive(false);
            audioPool.Add(audioSource);
        }
    }
    private AudioSource GetPooledAudioSource()
    {
        foreach (AudioSource source in audioPool)
        {
            if (!source.gameObject.activeInHierarchy)
            {
                source.gameObject.SetActive(true);
                return source;
            }
        }

        // Expand pool if needed
        AudioSource newSource = Instantiate(soundFXPrefab, transform);
        audioPool.Add(newSource);
        return newSource;
    }
    private IEnumerator DeactivateAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.gameObject.SetActive(false);
    }
    public void PlaySound(AudioClip audioClip, float volume = 1, Transform spawn = null, bool loop = false, float pitch = 1f)
    {
        if (currentSoundsPlaying >= maxSoundsPlaying)
        return;  // Do not play the sound if the limit is exceeded

        if (spawn == null)
        spawn = transform; // Default to this object's transform
        AudioSource audioSource = Instantiate(soundFXPrefab, spawn.position, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.Play();

        if (loop)
        {
            loopSources.Add(audioSource);
            return;
        }

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
    public void PlayRandomSound(AudioClip[] audioClip, float volume = 1, Transform spawn = null, bool loop = false, float pitch = 1f)
    {
        if (currentSoundsPlaying >= maxSoundsPlaying)
        return;  // Do not play the sound if the limit is exceeded

        int R = Random.Range(0, audioClip.Length);

        if (spawn == null)
        spawn = transform; // Default to this object's transform
        AudioSource audioSource = Instantiate(soundFXPrefab, spawn.position, Quaternion.identity);

        audioSource.clip = audioClip[R];
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.Play();

        if (loop)
        {
            loopSources.Add(audioSource);
            return;
        }

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
    public void StopSoundGradually(AudioClip audioClip, float fadeDuration = 2f)
    {
        StartCoroutine(FadeOutSound(audioClip, fadeDuration));
    }
    public IEnumerator FadeOutSound(AudioClip audioClip, float duration)
    {
        AudioSource audioSource = loopSources.Find(sources => sources.clip == audioClip);
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        Destroy(audioSource.gameObject);
    }
    public void StopAllLoopSources(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutAllLoopSources(fadeDuration));
    }
    public IEnumerator FadeOutAllLoopSources(float duration)
    {
        foreach (AudioSource audioSource in loopSources)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }
            Destroy(audioSource.gameObject);
        }
    }
}