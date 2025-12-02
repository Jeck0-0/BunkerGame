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
        GameClient.Subscribe<STC_StartEmergency>(OnStartEmergency);
        dilemmaLights.SetActive(false);
        crisisLights.SetActive(false);
        StartCoroutine(TurnLightOn());
    }

    private void OnDestroy()
    {
        GameClient.Unsubscribe<STC_StartEmergency>(OnStartEmergency);
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
        AudioManager.Instance.PlaySound(AudioStorage.Instance.GetFlickeringLightClip(), 0.8f, null, true);
        MusicManager.Instance.BlockMusic(false);
        crisisLights.SetActive(false);
        dilemmaLights.SetActive(true);
        Crisis = false;
    }

    private IEnumerator CrisisBuildUp()
    {
        dilemmaLights.SetActive(false);
        MusicManager.Instance.BlockMusic(true);
        AudioManager.Instance.StopSoundGradually(AudioStorage.Instance.GetFlickeringLightClip(), 0.01f);

        yield return new WaitForSeconds(1.5f);

        crisisLights.SetActive(true);
        if (siren) AudioManager.Instance.PlaySound(siren, 0.2f, null, true);
    }
    private IEnumerator TurnLightOn()
    {
        yield return new WaitForSeconds(0.5f);

        dilemmaLights.SetActive(true);
        yield return new WaitForSeconds(0.03f);
        dilemmaLights.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        dilemmaLights.SetActive(true);
        yield return new WaitForSeconds(0.03f);
        dilemmaLights.SetActive(false);
        yield return new WaitForSeconds(0.07f);
        dilemmaLights.SetActive(true);
        yield return new WaitForSeconds(0.03f);
        dilemmaLights.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        dilemmaLights.SetActive(true);

    }
}