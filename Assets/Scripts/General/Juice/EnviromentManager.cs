using UnityEngine;

public class EnviromentManager : MonoBehaviour
{
    [SerializeField] GameObject crisisLights;
    [SerializeField] GameObject dilemmaLights;
    [SerializeField] AudioClip siren;
    private bool Crisis = false;

    private void Awake()
    {
        // subscribe to events
    }

    public void OnCrisis()
    {
        if (Crisis) return;

        if (siren) AudioManager.Instance.PlaySound(siren, 0.3f, null, true);
        crisisLights.SetActive(true);
        dilemmaLights.SetActive(false);
        Crisis = true;
    }
    public void OnDilemma()
    {
        if (!Crisis) return;

        if (siren) AudioManager.Instance.StopSoundGradually(siren, 0f);
        crisisLights.SetActive(false);
        dilemmaLights.SetActive(true);
        Crisis = false;
    }
    // for testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnCrisis();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnDilemma();
        }
    }
}