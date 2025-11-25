using Client;
using Networking;
using Packets;
using System.Collections;
using UnityEngine;

public class EnviromentManager : MonoBehaviour
{
    [SerializeField] GameObject crisisLights;
    [SerializeField] GameObject dilemmaLights;
    [SerializeField] AudioClip siren;
    private bool Crisis = false;

    private void Awake()
    {
        NetworkManager.Client.Subscribe<STC_StartEmergency>(OnStartEmergency);
    }

    private void OnDestroy()
    {
        NetworkManager.Client?.Unsubscribe<STC_StartEmergency>(OnStartEmergency);
    }

    private void OnStartEmergency(BasePacket packet)
    {
        var data = (STC_StartEmergency)packet;

        if (data.emergencyType == EmergencyType.Crisis)
        {
            OnCrisis();
        }
        else
        {
            OnDilemma();
        }
    }

    public void OnCrisis()
    {
        if (Crisis) return;

        StartCoroutine(CrisisBuildUp());

        Crisis = true;
    }
    public void OnDilemma()
    {
        if (!Crisis) return;

        if (siren) AudioManager.Instance.StopSoundGradually(siren, 0f);
        MusicManager.Instance.BlockMusic(false);
        crisisLights.SetActive(false);
        dilemmaLights.SetActive(true);
        Crisis = false;
    }

    private IEnumerator CrisisBuildUp()
    {
        dilemmaLights.SetActive(false);
        MusicManager.Instance.BlockMusic(true);

        yield return new WaitForSeconds(1.5f);

        crisisLights.SetActive(true);
        if (siren) AudioManager.Instance.PlaySound(siren, 0.2f, null, true);
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