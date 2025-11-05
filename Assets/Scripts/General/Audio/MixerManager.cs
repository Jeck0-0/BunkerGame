using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    public void SetSoundVolume(float volume)
    {
        mixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20f);
    }
    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }
}
