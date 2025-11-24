using UnityEngine;
using System.Collections;
using TMPro;

public class TapePlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform tapeSlot;
    [SerializeField] TextMeshPro songName;

    [Header("Settings")]
    [SerializeField] float insertDelay = 0.4f;
    [SerializeField] float ejectDelay = 0.3f;
    [SerializeField] float buttonClickDelay = 0.15f;

    [Header("Sounds")]
    [SerializeField] AudioClip insertSound;
    [SerializeField] AudioClip ejectSound;
    [SerializeField] AudioClip[] buttonClickSounds;

    private AudioClip currentSong;
    private Tape insertedTape;
    private TapeObject tapeObject;
    private bool isBusy = false;
    private bool isPaused = false;
    private bool isLooping = false;
    public bool HasTape => insertedTape != null;
    public bool IsBusy => isBusy;

    public void InsertTape(Tape tape, TapeObject tapeObj)
    {
        if (isBusy || HasTape) return;
        if (tape == null || tapeObj == null)
        {
            Debug.LogWarning("no tape");
            return;
        }

        tapeObject = tapeObj;
        tapeObject.gameObject.SetActive(false);
        StartCoroutine(InsertRoutine(tape));
    }

    public void EjectTape()
    {
        if (buttonClickSounds.Length > 0) AudioManager.Instance.PlayRandomSound(buttonClickSounds, 1f);
        if (isBusy || !HasTape) return;

        StartCoroutine(EjectRoutine());
    }

    private IEnumerator InsertRoutine(Tape tape)
    {
        isBusy = true;

        //AudioManager.Instance.PlaySound(insertSound);
        yield return new WaitForSeconds(insertDelay);

        insertedTape = tape;
        MusicManager.Instance.InsertTape(tape, this);
        isPaused = false;

        isBusy = false;
    }

    private IEnumerator EjectRoutine()
    {
        isBusy = true;
        songName.text = "";
        tapeObject.gameObject.SetActive(true);
        MusicManager.Instance.StopCurrentMusic(0f);

        //AudioManager.Instance.PlaySound(ejectSound);
        yield return new WaitForSeconds(ejectDelay);

        insertedTape = null;
        isPaused = false;
        isLooping = false;

        isBusy = false;
    }

    public void PlayOrPause()
    {
        if (buttonClickSounds.Length > 0) AudioManager.Instance.PlayRandomSound(buttonClickSounds, 1f);
        if (!HasTape || isBusy) return;
        StartCoroutine(PlayPauseRoutine());
    }

    private IEnumerator PlayPauseRoutine()
    {
        isBusy = true;

        yield return new WaitForSeconds(buttonClickDelay);

        if (isPaused)
        {
            MusicManager.Instance.ResumeMusic();
            isPaused = false;
        }
        else
        {
            MusicManager.Instance.PauseMusic();
            isPaused = true;
        }

        isBusy = false;
    }

    public void Stop()
    {
        if (buttonClickSounds.Length > 0) AudioManager.Instance.PlayRandomSound(buttonClickSounds, 1f);
        if (!HasTape || isBusy) return;
        StartCoroutine(StopRoutine());
    }

    private IEnumerator StopRoutine()
    {
        isBusy = true;

        yield return new WaitForSeconds(buttonClickDelay);

        MusicManager.Instance.StopCurrentMusic();
        isPaused = false;

        isBusy = false;
    }

    public void NextTrack()
    {
        if (buttonClickSounds.Length > 0) AudioManager.Instance.PlayRandomSound(buttonClickSounds, 1f);
        if (!HasTape || isBusy) return;
        StartCoroutine(NextTrackRoutine());
    }

    private IEnumerator NextTrackRoutine()
    {
        isBusy = true;

        yield return new WaitForSeconds(buttonClickDelay);

        MusicManager.Instance.NextMusic();
        isPaused = false;

        isBusy = false;
    }

    public void ToggleLoop()
    {
        if (buttonClickSounds.Length > 0) AudioManager.Instance.PlayRandomSound(buttonClickSounds, 1f);
        if (!HasTape || isBusy) return;
        StartCoroutine(LoopRoutine());
    }

    private IEnumerator LoopRoutine()
    {
        isBusy = true;

        yield return new WaitForSeconds(buttonClickDelay);

        isLooping = !isLooping;
        MusicManager.Instance.LoopTrack(isLooping);
        Debug.Log($"TapePlayer: Loop mode {(isLooping ? "enabled" : "disabled")}");

        isBusy = false;
    }

    public void SetSong(AudioClip clip)
    {
        currentSong = clip;
        songName.text = currentSong.name;
    }
}