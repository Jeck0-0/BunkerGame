using UnityEngine;

public class InputRelay : MonoBehaviour
{
    [SerializeField] LayerMask RaycastMask = ~0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       Ray mouseray = Camera.main.ScreenPointToRay(Input.mousePosition);
       RaycastHit hitResault;
    }
}
