using UnityEngine;

public class CameraSoliderTest2 : MonoBehaviour
{

    public float sensitivity = 3f;

    float rotationX;

    float rotationY;

    void Update()
    {

        rotationX += Input.GetAxis("Mouse X") * sensitivity;

        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

        rotationY = Mathf.Clamp(rotationY, -80f, 80f);



        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }

    
}
