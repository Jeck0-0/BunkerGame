using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }
    public void PlaySound()
    {
        AudioManager.Instance.PlayRandomSound(AudioStorage.Instance.GetUIButtonClips(), 0.8f);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverSound();
    }
    public void HoverSound()
    {
        AudioManager.Instance.PlayRandomSound(AudioStorage.Instance.GetUIHoverClips(), 0.1f);
    }
}
