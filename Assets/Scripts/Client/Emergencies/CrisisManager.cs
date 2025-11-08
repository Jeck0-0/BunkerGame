using System.Collections;
using Networking;
using Packets;
using UnityEngine;

namespace Client
{
    public class CrisisManager : Singleton<CrisisManager>
    {
        private bool crisisFinished = false;
        
        public IEnumerator CrisisPhase(Crisis crisis)
        {
            Debug.Log("Started crisis: " + crisis.name);
            
            // show crisis ui
            
            // get contribution input
            CTS_ContributeToCrisis packet = new CTS_ContributeToCrisis(3);
            NetworkManager.Client.Send(packet);

            
            NetworkManager.Client.Subscribe<STC_CrisisResult>(OnCrisisResult);
            
            yield return new WaitUntil(() => crisisFinished);
            crisisFinished = false;
            
            NetworkManager.Client.Unsubscribe<STC_CrisisResult>(OnCrisisResult);
        }

        private void OnCrisisResult(BasePacket p)
        {
            STC_CrisisResult packet = (STC_CrisisResult)p;
            
            ClientTracks.Instance.ApplyModifier(packet.TrackMod);
            ClientResources.Instance.ModifyMaterials(packet.materialsMod);
                
            if (packet.success)
            {
                Debug.Log("Success!");
                //success UI
            }
            else
            {
                Debug.Log("Failed!");
                //fail UI
            }
        }
    }
}