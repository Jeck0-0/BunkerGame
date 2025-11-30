using UnityEngine;
using System.Collections;

public class LookCamera : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] bool lockCamera = false;
    [SerializeField] float sensitivity = 50f;
    [SerializeField] float returnSpeed = 6f;

    [Header("Rotation Limits")]
    [SerializeField] float minYaw = -70f;
    [SerializeField] float maxYaw = 70f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 45f;

    [Header("Zoom Out")]
    [SerializeField] Transform zoomInPos;
    [SerializeField] Transform zoomOutPos;
    [SerializeField] float zoomOutDuration = 1f;
    [SerializeField] AnimationCurve zoomOutCurve;

    private Quaternion originalRotation;
    private float yaw;
    private float pitch;
    public bool IsLooking { get; private set; }
    private bool isReturning;
    private bool zoomedOut = false;

    void Start()
    {
        transform.position = zoomInPos.position;
        if (lockCamera) return;

        originalRotation = transform.localRotation;
        Vector3 originalRot = transform.localEulerAngles;
        yaw = originalRot.y;
        pitch = originalRot.x;

        SetZoomedOut(true);
    }

    void Update()
    {
        if (!zoomedOut) return;

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
        yaw = transform.localRotation.eulerAngles.y;
        pitch = transform.localRotation.eulerAngles.x;

        if (Quaternion.Angle(transform.localRotation, originalRotation) < 0.1f)
        {
            transform.localRotation = originalRotation;
            yaw = originalRotation.eulerAngles.y;
            pitch = originalRotation.eulerAngles.x;
            isReturning = false;
        }
    }

    public void SetZoomedOut(bool state)
    {
        StopAllCoroutines();
        StartCoroutine(Zoom(state));
    }

    private IEnumerator Zoom(bool zoomOut)
    {
        if (!zoomOut) zoomedOut = false;

        Transform start = zoomOut ? zoomInPos : zoomOutPos;
        Vector3 startPos = start.position;
        Quaternion startRot = start.rotation;

        Transform target = zoomOut ? zoomOutPos : zoomInPos;
        Vector3 targetPos = target.position;
        Quaternion targetRot = target.rotation;

        float t = 0f;

        while (t < zoomOutDuration)
        {
            t += Time.deltaTime;
            float lerp = zoomOutCurve.Evaluate(t / zoomOutDuration);

            transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, lerp);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (zoomOut) zoomedOut = true;
    }
}