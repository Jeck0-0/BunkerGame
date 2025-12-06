using Networking;
using Packets;
using System.Collections.Generic;

public class NamePlatesUI : Singleton<NamePlatesUI>
{
    private Dictionary<uint, NamePlate> platesByPlayer = new();

    private void Start() 
    {
        GameClient.Subscribe<STC_PlayerVoted>(DisplayVote);
    }
    private void OnDestroy()
    {
        GameClient.Unsubscribe<CTS_VoteOnDilemma>(DisplayVote);
    }
    public void RegisterPlayerObj(PlayerInstance player)
    {
        var plate = player.obj.GetComponentInChildren<NamePlate>();
        platesByPlayer[player.info.id] = plate;
    }

    private void DisplayVote(BasePacket packet)
    {
        var p = packet as STC_PlayerVoted;
        if (p == null) return;


        if (platesByPlayer.TryGetValue(p.PlayerId, out var plate))
            plate.DisplayVote($"<color=white>Voted: {p.OptionIndex + 1}</color>");
    }

    public void NoVoting()
    {
        foreach(NamePlate plate in platesByPlayer.Values)
        {
            plate.DisplayVote("");
        }
    }
    public void AwatingVote()
    {
        foreach (NamePlate plate in platesByPlayer.Values)
        {
            plate.DisplayVote("<color=red>Didn't vote</color>");
        }
    }
}
