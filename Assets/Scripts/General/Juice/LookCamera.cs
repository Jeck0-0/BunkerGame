using UnityEngine;

public class LookCamera : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] float sensitivity = 50f;
    [SerializeField] float returnSpeed = 6f;

    [Header("Rotation Limits")]
    [SerializeField] float minYaw = -70f;
    [SerializeField] float maxYaw = 70f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 45f;

    private Quaternion originalRotation;
    private float yaw;
    private float pitch;
    public bool IsLooking { get; private set; }
    private bool isReturning;

    void Start()
    {
        originalRotation = transform.localRotation;
        Vector3 originalRot = transform.localEulerAngles;
        yaw = originalRot.y;
        pitch = originalRot.x;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsLooking = true;
            isReturning = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            IsLooking = false;
            isReturning = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (IsLooking)
        {
            HandleLook();
        }
        else if (isReturning)
        {
            Return();
        }
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        yaw += mouseX * sensitivity * Time.deltaTime;
        pitch -= mouseY * sensitivity * Time.deltaTime;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Return()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * returnSpeed);

        if (Quaternion.Angle(transform.localRotation, originalRotation) < 0.1f)
        {
            transform.localRotation = originalRotation;
            yaw = originalRotation.eulerAngles.y;
            pitch = originalRotation.eulerAngles.x;
            isReturning = false;
        }
    }
}
