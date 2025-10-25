//https://www.youtube.com/watch?v=fXsdK2umVmM toutorial followed for making this system
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReciver : MonoBehaviour
{
    [SerializeField] RectTransform CanvasTransform;
    GraphicRaycaster graphicRaycaster;

    private GameObject lastHoveredObject = null;

    List<GameObject> DragTargets = new List<GameObject>();
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    void Update()
    {

    }

    public void OnCursorInput(Vector2 normalisedPosition)
    {
        Vector3 MousePosition = new Vector3(CanvasTransform.sizeDelta.x * normalisedPosition.x,
                                            CanvasTransform.sizeDelta.y * normalisedPosition.y, 0f);
        //Debug.Log(MousePosition);

        PointerEventData MouseEvent = new PointerEventData(EventSystem.current);
        MouseEvent.position = MousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(MouseEvent, results);

        bool SendMouseDown = Input.GetMouseButtonDown(0);
        bool SendMouseUp = Input.GetMouseButtonUp(0);
        bool IsMouseDown = Input.GetMouseButton(0);

        if (SendMouseUp)
        {
            foreach (var target in DragTargets)
            {
                ExecuteEvents.Execute(target, MouseEvent, ExecuteEvents.endDragHandler);    
            }
            DragTargets.Clear();
        }



        foreach (var result in results)
        {
            GameObject hoveredObject = result.gameObject;

            PointerEventData EventData = new PointerEventData(EventSystem.current);
            EventData.position = MousePosition;
            EventData.pointerCurrentRaycast = EventData.pointerPressRaycast = result;

            if (SendMouseDown)
            {
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.pointerDownHandler);
            }

            if (SendMouseDown)
            {
                DragTargets.Add(result.gameObject);
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.beginDragHandler);
            }
            else if (DragTargets.Contains(result.gameObject))
            {
                EventData.dragging=true;
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.dragHandler);
            }

            if (IsMouseDown)
            {
                EventData.button = PointerEventData.InputButton.Left;
            }
            //Debug.Log(result.gameObject.name);

            else if (SendMouseUp)
            {
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.pointerClickHandler);
            }
            if (hoveredObject != lastHoveredObject)
            {
                if (lastHoveredObject != null)
                    ExecuteEvents.Execute(lastHoveredObject, EventData, ExecuteEvents.pointerExitHandler);

                if (hoveredObject != null)
                    ExecuteEvents.Execute(hoveredObject, EventData, ExecuteEvents.pointerEnterHandler);

                lastHoveredObject = hoveredObject;
            }
        }

    }

    public void Buttontest()
    {
        Debug.Log($"PRESSED!!");
    }

}
