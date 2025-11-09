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
        }
        public void ModifyMaterials(int amount)
        {
            resources.ModifyMaterials(amount);
        }
    }
}