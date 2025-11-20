using UnityEngine;

public class temporaryJump : MonoBehaviour // for playtest
{
    public float AppearTime = 5f;
    public GameObject obj;
    public Animation anim;
    private void Start()
    {
        obj.SetActive(false);
        Invoke("appeartimer", 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) 
        {
            anim.Play("jumpscare");
        }
    }
    private void appeartimer() 
    {
        AppearTime = Random.Range(5f, 10f);
        Invoke("appear", AppearTime);
    }
    private void appear()
    {
        obj.SetActive(true);
        anim.Play("jumpscare");
        Invoke("walkback", 10f);
    }

    private void walkback()
    {
        anim.Play("walkback");
        Invoke("disappier", 2f);
    }
    private void disappier()
    {
        obj.SetActive(false);
        Invoke("appeartimer", 20f);
    }
}