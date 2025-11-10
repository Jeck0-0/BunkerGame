using Networking;
using Packets;
using UnityEngine;

namespace Client
{
    public class ClientResources : Singleton<ClientResources>,  IPlayerResources
    {
        private PlayerResources resources = new PlayerResources();
        public int Influence => resources.Influence;
        public int Materials => resources.Materials;

        public void ModifyInfluence(int amount)
        {
            resources.ModifyInfluence(amount);
            ClientResourcesUI.Instance.UpdateInfluenceUI(Influence);
        }
        public void ModifyMaterials(int amount)
        {
            resources.ModifyMaterials(amount);
            ClientResourcesUI.Instance.UpdateMaterialsUI(Materials);
        }

        protected override void Awake()
        {
            NetworkManager.Client.Subscribe<STC_UpdateResources>(OnUpdateResources);
            base.Awake();
        }

        private void OnDestroy()
        {
            NetworkManager.Client.Unsubscribe<STC_UpdateResources>(OnUpdateResources);
        }

        private void OnUpdateResources(BasePacket p)
        {
            var packet = (STC_UpdateResources)p;
            resources.SetMaterials(packet.materials);
            resources.SetInfluence(packet.influence);

            ClientResourcesUI.Instance.UpdateInfluenceUI(Influence);
            ClientResourcesUI.Instance.UpdateMaterialsUI(Materials);
            Debug.Log($"[ClientResources] Updated: Materials = {Materials}, Influence = {Influence}");
        }
    }
}