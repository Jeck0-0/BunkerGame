using TMPro;
using UnityEngine;

public class NamePlate : MonoBehaviour
{
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro voteText;

    public uint OwnerId { get; private set; }

    public void Initialize(uint ownerId, string playerName)
    {
        OwnerId = ownerId;
        nameText.text = playerName;
        voteText.text = "";
    }

    public void DisplayVote(string vote)
    {
        voteText.text = vote;
    }
}
