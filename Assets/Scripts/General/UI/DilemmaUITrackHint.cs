using UnityEngine;
using UnityEngine.UI;

public class DilemmaUITrackHint : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject upHint;
    [SerializeField] GameObject downHint;
    [SerializeField] Image hint;

    [Header("Resource Sprites")]
    [SerializeField] Sprite orderSprite;
    [SerializeField] Sprite populationSprite;
    [SerializeField] Sprite foodSprite;
    [SerializeField] Sprite energySprite;
    [SerializeField] Sprite moraleSprite;

    public void InitializeHint(TrackType track, bool up)
    {
        Sprite trackSprite = orderSprite;

        switch (track)
        {
            case TrackType.Order:
                trackSprite = orderSprite;
                break;
            case TrackType.Population:
                trackSprite = populationSprite;
                break;
            case TrackType.Food:
                trackSprite = foodSprite;
                break;
            case TrackType.Energy:
                trackSprite = energySprite;
                break;
            case TrackType.Moral:
                trackSprite = moraleSprite;
                break;
        }

        hint.sprite = trackSprite;
        if (up) upHint.SetActive(true);
        else downHint.SetActive(true);
    }
}