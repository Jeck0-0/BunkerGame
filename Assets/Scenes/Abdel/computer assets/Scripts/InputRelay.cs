using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputRelay : MonoBehaviour
{
    [SerializeField] LayerMask RaycastMask = ~0;
    [SerializeField] float RaycastDistance = 5f;
    [SerializeField] UnityEvent<Vector2> OnCursorInput = new UnityEvent<Vector2>();

    void Update()
    {
        Ray mouseray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(mouseray.origin, mouseray.direction * RaycastDistance, Color.red);

        RaycastHit HitResault;
        if (Physics.Raycast(mouseray, out HitResault, RaycastDistance, RaycastMask, QueryTriggerInteraction.Ignore))
        {
            if (HitResault.collider.gameObject != gameObject)
            {
                return;
            }    
            InputReciver.Instance.OnCursorInput(HitResault.textureCoord);
            //Debug.Log(HitResault.textureCoord);
            //Debug.Log(HitResault.transform.gameObject);
        }
    }
}
