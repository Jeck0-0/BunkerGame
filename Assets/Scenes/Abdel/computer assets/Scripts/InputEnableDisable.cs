using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class InputEnableDisable : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private InputSystemUIInputModule inputSytem;

    public void OnSelect(BaseEventData eventData)
    {
        if (inputSytem != null)
        {
            inputSytem.enabled = true;
            Debug.Log("enabled");
        }
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (inputSytem != null)
        {
            inputSytem.enabled = false;
            Debug.Log("disabled");
        }
    }
}
