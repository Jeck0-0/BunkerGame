using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] Light[] lights;
    [SerializeField] float baseIntensity = 2f;
    [SerializeField] float flickerAmount = 0.5f;
    [SerializeField] float flickerSpeed = 5f;
    [SerializeField] float randomness = 1f;

    private float[] _phaseOffsets;

    void Start()
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogWarning($"{nameof(LightFlicker)}: No lights assigned.");
            return;
        }

        _phaseOffsets = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            _phaseOffsets[i] = Random.Range(0f, 100f);
        }
    }

    void Update()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;

            float noise = Mathf.PerlinNoise((Time.time * flickerSpeed) + _phaseOffsets[i], 0f);
            float intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
            lights[i].intensity = Mathf.Max(0f, intensity);

            if (Random.value < 0.005f)
            lights[i].intensity *= Random.Range(0.4f, 0.8f);
        }
    }
}
