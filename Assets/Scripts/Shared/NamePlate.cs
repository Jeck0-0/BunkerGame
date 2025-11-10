using TMPro;
using UnityEngine;

public class NamePlate : MonoBehaviour
{
    [SerializeField] TextMeshPro playerName;
    [SerializeField] Transform emblemParent;

    public void DisplayPlayer(EmblemData data)
    {
        playerName.text = data.FactionName;
        EmblemBuilder.Instance.BuildWorldEmblem(data, emblemParent);
    }
}
