using System.Collections;
using TMPro;
using UnityEngine;

public class ClientResourcesUI : Singleton<ClientResourcesUI>
{
    [SerializeField] TextMeshProUGUI influenceCounter;
    [SerializeField] TextMeshProUGUI materialsCounter;
    [SerializeField] float incomeRevealDuration = 1f;

    [SerializeField] TextMeshProUGUI influenceChangeText;
    [SerializeField] TextMeshProUGUI materialsChangeText;

    public void UpdateInfluenceUI(int influence) => influenceCounter.text = influence.ToString();
    public void UpdateMaterialsUI(int materials) => materialsCounter.text = materials.ToString();

    public void DisplayInfluenceIncome(int amount)
    {
        StartCoroutine(FlashResourceChange(influenceChangeText, amount));
    }

    public void DisplayMaterialsIncome(int amount)
    {
        StartCoroutine(FlashResourceChange(materialsChangeText, amount));
    }

    private IEnumerator FlashResourceChange(TextMeshProUGUI textObj, int amount)
    {
        GameObject actualText = textObj == influenceChangeText ? influenceCounter.gameObject : materialsCounter.gameObject;
        actualText.SetActive(false);
        textObj.gameObject.SetActive(true);
        textObj.text = amount > 0 ? $"+{amount}" : amount.ToString();
        textObj.color = amount > 0 ? Color.green : Color.red;
        textObj.alpha = 1f;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / incomeRevealDuration;
            textObj.alpha = 1f - t;
            yield return null;
        }

        textObj.gameObject.SetActive(false);
        actualText.SetActive(true);
    }
}