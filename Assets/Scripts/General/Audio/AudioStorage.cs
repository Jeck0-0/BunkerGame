using UnityEngine;

public class AudioStorage : Singleton<AudioStorage>
{
    [SerializeField] AudioClip[] writingClips;
    public AudioClip[] GetWritingClips() => writingClips;

    [SerializeField] AudioClip[] UIButtonClips;
    public AudioClip[] GetUIButtonClips() => UIButtonClips;

    [SerializeField] AudioClip[] UIHoverClips;
    public AudioClip[] GetUIHoverClips() => UIHoverClips;

    [SerializeField] AudioClip[] paperClips;
    public AudioClip[] GetPaperClips() => paperClips;

    [SerializeField] AudioClip computerSlidingClip;
    public AudioClip GetComputerSlidingClip() => computerSlidingClip;

    [SerializeField] AudioClip computerDoorsClip;
    public AudioClip GetComputerDoorsClip() => computerDoorsClip;

    [SerializeField] AudioClip flickeringLightClip;
    public AudioClip GetFlickeringLightClip() => flickeringLightClip;

    [SerializeField] AudioClip powerShutdownClip;
    public AudioClip GetPowerShutdownClip() => powerShutdownClip;

    [SerializeField] AudioClip ambientClip;
    public AudioClip GetAmbientClip() => ambientClip;
}
