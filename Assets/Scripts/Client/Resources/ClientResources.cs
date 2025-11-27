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
            int oldValue = Influence;
            resources.ModifyInfluence(amount);
            int change = Influence - oldValue;

            ClientResourcesUI.Instance.UpdateInfluenceUI(Influence);
            if (change != 0)
                ClientResourcesUI.Instance.DisplayInfluenceIncome(change);
        }

        public void ModifyMaterials(int amount)
        {
            int oldValue = Materials;
            resources.ModifyMaterials(amount);
            int change = Materials - oldValue;

            ClientResourcesUI.Instance.UpdateMaterialsUI(Materials);
            if (change != 0)
                ClientResourcesUI.Instance.DisplayMaterialsIncome(change);
        }

        public void SetInfluence(int amount)
        {
            int oldValue = Influence;
            resources.SetInfluence(amount);
            int change = Influence - oldValue;

            ClientResourcesUI.Instance.UpdateInfluenceUI(Influence);
            if (change != 0)
                ClientResourcesUI.Instance.DisplayInfluenceIncome(change);
        }

        public void SetMaterials(int amount)
        {
            int oldValue = Materials;
            resources.SetMaterials(amount);
            int change = Materials - oldValue;

            ClientResourcesUI.Instance.UpdateMaterialsUI(Materials);
            if (change != 0)
                ClientResourcesUI.Instance.DisplayMaterialsIncome(change);
        }

        protected override void Awake()
        {
            GameClient.Subscribe<STC_UpdateResources>(OnUpdateResources);
            base.Awake();
        }

        private void OnDestroy()
        {
            GameClient.Unsubscribe<STC_UpdateResources>(OnUpdateResources);
        }

        private void OnUpdateResources(BasePacket p)
        {
            var packet = (STC_UpdateResources)p;
            SetInfluence(packet.influence);
            SetMaterials(packet.materials);
            Debug.Log($"[ClientResources] Updated: Materials = {Materials}, Influence = {Influence}");
        }
    }
}