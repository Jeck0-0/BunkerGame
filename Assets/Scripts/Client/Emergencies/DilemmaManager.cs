using Client;
using Networking;
using Packets;
using System.Collections;
using UnityEngine;

public class DilemmaManager : Singleton<DilemmaManager>
{
    private STC_DilemmaResult currentDillemma;
    private bool resultReceived;

    public IEnumerator DilemmaPhase(Dilemma dilemma)
    {
        Debug.Log("Started dilemma: " + dilemma.name);

        // Show dilemma UI
        DilemmaUI.Instance.DisplayDilemma(dilemma);

        resultReceived = false;
        currentDillemma = null;

        void OnDilemmaResult(BasePacket p)
        {
            var packet = p as STC_DilemmaResult;
            if (packet == null) return;

            currentDillemma = packet;
            resultReceived = true;

            // Apply track modifiers immediately
            ClientTracks.Instance.ApplyModifier(packet.TrackModifier);
            DilemmaUI.Instance.DisplayResult(packet);
        }

        NetworkManager.Client.Subscribe<STC_DilemmaResult>(OnDilemmaResult);

        yield return new WaitUntil(() => resultReceived);

        NetworkManager.Client.Unsubscribe<STC_DilemmaResult>(OnDilemmaResult);

        yield return new WaitForSeconds(3f);

        Debug.Log("Dilemma finished: " + dilemma.name);
    }
}
