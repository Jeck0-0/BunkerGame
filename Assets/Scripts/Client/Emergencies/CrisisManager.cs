using Networking;
using Packets;
using System.Collections;
using UnityEngine;

namespace Client
{
    public class CrisisManager : Singleton<CrisisManager>
    {
        //private bool crisisFinished = false;
        private STC_CrisisResult currentResult;
        private bool resultReceived;

        public IEnumerator CrisisPhase(Crisis crisis)
        {
            Debug.Log("Started crisis: " + crisis.name);

            // show crisis ui
            CrisisUI.Instance.DisplayCrisis(crisis);

            // Do this with crisis UI
            /*
            CTS_ContributeToCrisis packet = new CTS_ContributeToCrisis(3);
            NetworkManager.Client.Send(packet);
            */

            // wait for packet
            resultReceived = false;
            currentResult = null;

            void OnCrisisResult(BasePacket p)
            {
                var packet = p as STC_CrisisResult;
                if (packet == null) return;

                currentResult = packet;
                resultReceived = true;

                // Apply resources immediately
                ClientTracks.Instance.ApplyModifier(packet.TrackMod);
                ClientResources.Instance.ModifyMaterials(packet.materialsMod);

                CrisisUI.Instance.DisplayCrisisResult(packet.success, packet.TrackMod);
            }

            NetworkManager.Client.Subscribe<STC_CrisisResult>(OnCrisisResult);

            yield return new WaitUntil(() => resultReceived);
            //crisisFinished = false;
            
            NetworkManager.Client.Unsubscribe<STC_CrisisResult>(OnCrisisResult);

            yield return new WaitForSeconds(3f);

            CrisisUI.Instance.SlideOut();
            Debug.Log("Crisis finished: " + crisis.name);
        }

        /*
        private void OnCrisisResult(BasePacket p)
        {
            STC_CrisisResult packet = (STC_CrisisResult)p;
            
            ClientTracks.Instance.ApplyModifier(packet.TrackMod);
            ClientResources.Instance.ModifyMaterials(packet.materialsMod);

            CrisisUI.Instance.DisplayCrisisResult(packet.success, packet.TrackMod);
            ClientResources.Instance.ModifyMaterials(packet.materialsMod);
        }
        */
    }
}