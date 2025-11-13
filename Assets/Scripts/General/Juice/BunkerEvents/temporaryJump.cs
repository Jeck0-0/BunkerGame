using UnityEngine;

public class temporaryJump : MonoBehaviour // for playtest
{
    public float AppearTime = 5f;
    public GameObject obj;

    private void Start()
    {
        obj.SetActive(false);
        Invoke("appear", AppearTime);
    }
    private void appear()
    {
        obj.SetActive(true);
        Invoke("disappier", 10f);
    }
    private void disappier()
    {
        obj.SetActive(false);
    }
}