using UnityEngine;



public class HeadRotationLimit : MonoBehaviour
{

    public Transform cameraTransform;

    public float maxDown = 10f;

    public float maxUp = 12f;

    public float maxSide = 30f;

    public float maxTilt = 15f;


    Quaternion initialLocalRot;

    void Start()
    {

        initialLocalRot = transform.localRotation;

    }


    void LateUpdate()
    {

        if (!cameraTransform)
        {
            return;
        }


        Quaternion desiredLocal = Quaternion.Inverse(transform.parent.rotation) * cameraTransform.rotation;  //local camera

        Quaternion delta = desiredLocal * Quaternion.Inverse(initialLocalRot);


        Vector3 angles = delta.eulerAngles;

        angles = NormalizeEuler(angles);


        float x = Mathf.Clamp(angles.x, -maxUp, maxDown);

        float y = Mathf.Clamp(angles.y, -maxSide, maxSide);

        float z = Mathf.Clamp(angles.z, -maxTilt, maxTilt);


        Quaternion limitedDelta = Quaternion.Euler(x, y, z);

        transform.localRotation = limitedDelta * initialLocalRot;
    }

    Vector3 NormalizeEuler(Vector3 euler)  //euler angles
    {

        if (euler.x > 180f)
        {
            euler.x -= 360f;
        }

        if (euler.y > 180f)
        {
            euler.y -= 360f;
        }

        if (euler.z > 180f)
        {
            euler.z -= 360f;
        }


        return euler;
    }


    //public Transform target;

    //public float maxAngle = 30f;

    //private Quaternion initialLocalRot;

    //void Start()
    //{

    //    initialLocalRot = transform.localRotation;



    //}



    //void LateUpdate()
    //{


    //    Vector3 dir = target.position - transform.position;  //target of head

    //    Quaternion lookRot = Quaternion.LookRotation(dir);


    //    Quaternion localLook = Quaternion.Inverse(transform.parent.rotation) * lookRot;  //local coordinates tranform


    //    Quaternion limited = LimitRotation(localLook, maxAngle);  //limit


    //    transform.localRotation = limited;

    //}

    //Quaternion LimitRotation(Quaternion rot, float limit)
    //{

    //    rot.ToAngleAxis(out float angle, out Vector3 axis);

    //    if (angle > 180f)
    //    {

    //        angle -= 360f;

    //    }

    //    float clamped = Mathf.Clamp(angle, -limit, limit);

    //    return Quaternion.AngleAxis(clamped, axis);

    //}


}
