using UnityEngine;

public class SwayingLight : MonoBehaviour
{
    [SerializeField] float baseSwayAngle = 10f;
    [SerializeField] float swayVariation = 5f;
    [SerializeField] float swaySpeed = 1f;
    [SerializeField] float angleChangeInterval = 2f;
    [SerializeField] Vector3 swayAxis = Vector3.up;

    private Quaternion startRotation;
    private float phaseOffset;
    private float currentTargetAngle;
    private float nextTargetAngle;
    private float angleLerp;
    private float speedVariation;

    void Start()
    {
        startRotation = transform.localRotation;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        speedVariation = Random.Range(0.8f, 1.2f);

        currentTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
        nextTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
    }

    void Update()
    {
        angleLerp += Time.deltaTime / angleChangeInterval;
        if (angleLerp >= 1f)
        {
            angleLerp = 0f;
            currentTargetAngle = nextTargetAngle;
            nextTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
        }

        float swayAngle = Mathf.Lerp(currentTargetAngle, nextTargetAngle, angleLerp);
        float angle = Mathf.Sin((Time.time * swaySpeed * speedVariation) + phaseOffset) * swayAngle;

        Quaternion swayRotation = Quaternion.AngleAxis(angle, swayAxis.normalized);
        transform.localRotation = startRotation * swayRotation;
    }
}