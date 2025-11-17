using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Edge Settings")]
    [SerializeField] float edgeThreshold = 0.2f;
    [SerializeField] float sensitivity = 30f;

    [Header("Rotation Limits")]
    [SerializeField] float minYaw = -45f;
    [SerializeField] float maxYaw = 45f;
    [SerializeField] float minPitch = -10f;
    [SerializeField] float maxPitch = 25f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 initialRot = transform.localEulerAngles;
        yaw = initialRot.y;
        pitch = initialRot.x;
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;
        float screenW = Screen.width;
        float screenH = Screen.height;

        float leftEdge = screenW * edgeThreshold;
        float rightEdge = screenW * (1f - edgeThreshold);
        float topEdge = screenH * (1f - edgeThreshold);
        float bottomEdge = screenH * edgeThreshold;

        float yawInput = 0f;
        float pitchInput = 0f;

        // Horizontal
        if (mouse.x < leftEdge)
            yawInput = -1f;

        else if (mouse.x > rightEdge)
            yawInput = 1f;

        // Vertical
        if (mouse.y < bottomEdge)
            pitchInput = 1f;

        else if (mouse.y > topEdge)
            pitchInput = -1f;

        yaw += yawInput * sensitivity * Time.deltaTime;
        pitch += pitchInput * sensitivity * Time.deltaTime;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
