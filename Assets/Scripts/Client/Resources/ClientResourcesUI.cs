using TMPro;
using UnityEngine;

public class ClientResourcesUI : Singleton<ClientResourcesUI>
{
    [SerializeField] TextMeshProUGUI influenceCounter;
    [SerializeField] TextMeshProUGUI materialsCounter;

    public void UpdateInfluenceUI(float influence) => influenceCounter.text = influence.ToString();
    public void UpdateMaterialsUI(float materials) => materialsCounter.text = materials.ToString();
}