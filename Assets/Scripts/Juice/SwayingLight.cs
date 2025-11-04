using UnityEngine;

public class SwayingLight : MonoBehaviour
{
    [SerializeField] float baseSwayAngle = 10f;
    [SerializeField] float swayVariation = 5f;
    [SerializeField] float swaySpeed = 1f;
    [SerializeField] float angleChangeInterval = 2f;
    [SerializeField] Vector3 swayAxis = Vector3.up;

    private Quaternion _startRotation;
    private float _phaseOffset;
    private float _currentTargetAngle;
    private float _nextTargetAngle;
    private float _angleLerp;
    private float _speedVariation;

    void Start()
    {
        _startRotation = transform.localRotation;
        _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        _speedVariation = Random.Range(0.8f, 1.2f);

        _currentTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
        _nextTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
    }

    void Update()
    {
        _angleLerp += Time.deltaTime / angleChangeInterval;
        if (_angleLerp >= 1f)
        {
            _angleLerp = 0f;
            _currentTargetAngle = _nextTargetAngle;
            _nextTargetAngle = baseSwayAngle + Random.Range(-swayVariation, swayVariation);
        }

        float swayAngle = Mathf.Lerp(_currentTargetAngle, _nextTargetAngle, _angleLerp);
        float angle = Mathf.Sin((Time.time * swaySpeed * _speedVariation) + _phaseOffset) * swayAngle;

        Quaternion swayRotation = Quaternion.AngleAxis(angle, swayAxis.normalized);
        transform.localRotation = _startRotation * swayRotation;
    }
}