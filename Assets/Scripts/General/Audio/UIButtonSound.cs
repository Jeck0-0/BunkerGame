using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }
    public void PlaySound()
    {
        AudioManager.Instance.PlayRandomSound(AudioStorage.Instance.GetUIButtonClips(), 0.8f);
    }
}
