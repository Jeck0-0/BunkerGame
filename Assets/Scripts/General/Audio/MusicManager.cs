using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
    [Header("Audio")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] float defaultFadeTime = 0.5f;

    [Header("Debug")]
    [SerializeField] Tape currentTape;

    private Queue<AudioClip> musicQue = new Queue<AudioClip>();
    private AudioClip currentTrack;
    private Coroutine fadeCoroutine;
    private bool loopTrack = false;
    private TapePlayer player;


    public void PauseMusic() => musicSource.Pause();

    public void ResumeMusic() => musicSource.UnPause();

    public void LoopTrack(bool loop) => loopTrack = loop;

    public void NextMusic()
    {
        StopCurrentMusic(defaultFadeTime);
        PlayNextTrack();
    }

    public void ChangeMusicQue(bool shuffle)
    {
        PrepareTapeQueue(currentTape, shuffle);
    }

    public void InsertTape(Tape newTape, TapePlayer tapePlayer = null)
    {
        StopAllCoroutines();
        StopCurrentMusic(0f);

        currentTape = newTape;
        PrepareTapeQueue(newTape);
        if (tapePlayer != null)
        player = tapePlayer;
        PlayNextTrack();
    }

    private void PrepareTapeQueue(Tape tape, bool shuffle = true)
    {
        musicQue.Clear();

        if (tape == null || tape.Tracks == null || tape.Tracks.Length == 0)
        {
            Debug.LogWarning("Inserted tape has no tracks!");
            return;
        }

        List<AudioClip> shuffled = new List<AudioClip>(tape.Tracks);

        if (shuffle) // Randomize track order
        {
            for (int i = 0; i < shuffled.Count; i++)
            {
                AudioClip temp = shuffled[i];
                int randomIndex = Random.Range(i, shuffled.Count);
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }
        }

        foreach (var clip in shuffled)
        musicQue.Enqueue(clip);
    }

    private void PlayNextTrack()
    {
        if (loopTrack && currentTrack != null)
        {
            StartMusic(currentTrack, 1f, defaultFadeTime, loop: false);
            StartCoroutine(WaitForTrackEnd(currentTrack.length, () => PlayNextTrack()));
            return;
        }

        if (musicQue.Count == 0)
        {
            Debug.Log("End of tape reached");
            PrepareTapeQueue(currentTape, false);
            PlayNextTrack();
            return;
        }

        currentTrack = musicQue.Dequeue();
        StartMusic(currentTrack, 1f, defaultFadeTime, loop: false);
        player.SetSong(currentTrack);

        StartCoroutine(WaitForTrackEnd(currentTrack.length, () => PlayNextTrack()));
    }

    public void StartMusic(AudioClip clip, float volume = 1f, float fadeInTime = 0.5f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("no music clip");
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolume(volume, fadeInTime));
    }

    public void StopCurrentMusic(float fadeOutTime = 0.5f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeOutTime));
    }

    private IEnumerator FadeVolume(float targetVolume, float time)
    {
        float startVol = musicSource.volume;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, targetVolume, t / time);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop(float time)
    {
        float startVol = musicSource.volume;
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / time);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = 0f;
    }

    private IEnumerator WaitForTrackEnd(float duration, System.Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }
}