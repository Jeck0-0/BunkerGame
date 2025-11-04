using System;
using Networking;
using UnityEngine;

namespace Client
{
    public class EmergencyManager : MonoBehaviour
    {
        Emergency _currentEmergency;
        
        private void Start()
        {
            NetworkManager.Client.Subscribe<STC_StartEmergency>(OnCrisisStart);
        }

        protected void OnCrisisStart(BasePacket p)
        {
            STC_StartEmergency packet = (STC_StartEmergency)p;
            _currentEmergency = Resources.Load<Emergency>("ScriptableObjects/Crisis/" + packet.crisisId);

            switch (_currentEmergency.Type)
            {
                case EmergencyType.Crisis: //start crisis
                    break;
                case EmergencyType.Dilemma: //start dilemma
                    break;
                default:
                    Debug.LogError("Unknown Crisis");
                    break;
            } 
        }
    }
}