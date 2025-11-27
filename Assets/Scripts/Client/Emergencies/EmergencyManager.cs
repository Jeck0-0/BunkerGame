using System;
using Networking;
using Packets;
using UnityEngine;

namespace Client
{
    public class EmergencyManager : MonoBehaviour
    {
        Emergency _currentEmergency;
        
        private void Start()
        {
            GameClient.Subscribe<STC_StartEmergency>(OnCrisisStart);
            GameClient.Subscribe<STC_CrisisResult>(OnCrisisResult);
            GameClient.Subscribe<STC_DilemmaResult>(OnDilemmaResult);
        }

        private void OnDestroy()
        {
            GameClient.Unsubscribe<STC_StartEmergency>(OnCrisisStart);
            GameClient.Unsubscribe<STC_CrisisResult>(OnCrisisResult);
            GameClient.Unsubscribe<STC_DilemmaResult>(OnDilemmaResult);
        }

        protected void OnCrisisStart(BasePacket p)
        {
            STC_StartEmergency packet = (STC_StartEmergency)p;
            _currentEmergency = Resources.Load<Emergency>("ScriptableObjects/Emergencies/" + packet.emergencyType.ToString() + "/" + packet.crisisId);

            switch (_currentEmergency.Type)
            {
                case EmergencyType.Crisis: //start crisis
                    StartCoroutine(CrisisManager.Instance.CrisisPhase(_currentEmergency as Crisis));
                    break;
                case EmergencyType.Dilemma: //start dilemma
                    StartCoroutine(DilemmaManager.Instance.DilemmaPhase(_currentEmergency as Dilemma));
                    break;
                default:
                    Debug.LogError("Unknown Crisis");
                    break;
            } 
        }
        private void OnCrisisResult(BasePacket p)
        {
            var packet = (STC_CrisisResult)p;
            CrisisUI.Instance.DisplayCrisisResult(packet.success, packet.TrackMod);
        }

        private void OnDilemmaResult(BasePacket p)
        {
            var packet = (STC_DilemmaResult)p;
            DilemmaUI.Instance.DisplayResult(packet);
        }
    }
}