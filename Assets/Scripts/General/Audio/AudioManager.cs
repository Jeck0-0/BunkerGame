using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [HideInInspector] public List<AudioSource> loopSources;
    private List<AudioSource> audioPool = new List<AudioSource>();

    [SerializeField] AudioSource soundFXPrefab;
    [SerializeField] AudioMixerGroup SFXGroup;
    [SerializeField] int maxSoundsPlaying = 25;
    private int currentSoundsPlaying = 0;

    private void Start()
    {
        InitializeAudioPool();
    }

    public void PlayPooledSound(AudioClip clip, float volume = 1f, float pitch = 1f) // for sounds that should be played instantly
    {
        AudioSource audioSource = GetPooledAudioSource();
        audioSource.outputAudioMixerGroup = SFXGroup;
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
        if (currentSoundsPlaying >= maxSoundsPlaying || audioClip == null)
        return;  // Do not play the sound if the limit is exceeded

        if (spawn == null)
        spawn = transform; // Default to this object's transform
        AudioSource audioSource = Instantiate(soundFXPrefab, spawn.position, Quaternion.identity);

        audioSource.outputAudioMixerGroup = SFXGroup;
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
        if (currentSoundsPlaying >= maxSoundsPlaying || audioClip.Length == 0)
        return;  // Do not play the sound if the limit is exceeded

        int R = Random.Range(0, audioClip.Length);

        if (spawn == null)
        spawn = transform; // Default to this object's transform
        AudioSource audioSource = Instantiate(soundFXPrefab, spawn.position, Quaternion.identity);

        audioSource.outputAudioMixerGroup = SFXGroup;
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
        AudioSource audioSource = loopSources.Find(s => s != null && s.clip == audioClip);
        if (audioSource == null)yield break;

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration && audioSource != null)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            loopSources.Remove(audioSource);
            Destroy(audioSource.gameObject);
        }
    }
    public void StopAllLoopSources(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutAllLoopSources(fadeDuration));
    }
    public IEnumerator FadeOutAllLoopSources(float duration)
    {
        // to avoid directly modifying the list
        var sources = new List<AudioSource>(loopSources);

        foreach (AudioSource audioSource in sources)
        {
            if (audioSource == null) continue;
            yield return FadeOutSingleSource(audioSource, duration);
        }

        loopSources.Clear();
    }
    private IEnumerator FadeOutSingleSource(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration && source != null)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (source != null)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
}