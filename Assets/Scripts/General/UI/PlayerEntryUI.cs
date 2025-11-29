using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntryUI : MonoBehaviour
{
    public Button ButtonReference;
    [SerializeField] TextMeshProUGUI playerName;

    public void InitializeButton(string playerName, bool isHost = false)
    {
        this.playerName.text = playerName;

        if (isHost) ButtonReference.gameObject.SetActive(false);
        else ButtonReference.gameObject.SetActive(true);
    }
}
