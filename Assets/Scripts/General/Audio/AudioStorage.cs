using UnityEngine;

public class AudioStorage : Singleton<AudioStorage>
{
    [SerializeField] AudioClip[] writingClips;
    public AudioClip[] GetWritingClips() => writingClips;

    [SerializeField] AudioClip[] UIButtonClips;
    public AudioClip[] GetUIButtonClips() => UIButtonClips;

    [SerializeField] AudioClip[] paperClips;
    public AudioClip[] GetPaperClips() => paperClips;

    [SerializeField] AudioClip computerSlidingClip;
    public AudioClip GetComputerSlidingClip() => computerSlidingClip;

    [SerializeField] AudioClip computerDoorsClip;
    public AudioClip GetComputerDoorsClip() => computerDoorsClip;

    [SerializeField] AudioClip ambientClip;

    private void Start()
    {
       if (ambientClip) AudioManager.Instance.PlaySound(ambientClip, 0.6f, null, true);
    }
}
