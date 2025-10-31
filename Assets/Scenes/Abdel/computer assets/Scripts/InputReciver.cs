//https://www.youtube.com/watch?v=fXsdK2umVmM toutorial followed for making this system
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReciver : Singleton<InputReciver>
{
    [SerializeField] RectTransform CanvasTransform;
    GraphicRaycaster graphicRaycaster;

    private GameObject lastHoveredObject = null;

    List<GameObject> DragTargets = new List<GameObject>();
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    public void OnCursorInput(Vector2 normalisedPosition)
    {
        Debug.Log("In On input");
        Vector3 MousePosition = new Vector3(CanvasTransform.sizeDelta.x * normalisedPosition.x,
                                            CanvasTransform.sizeDelta.y * normalisedPosition.y, 0f);

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
        {Debug.Log(result.gameObject.name);
            GameObject hoveredObject = result.gameObject;

            PointerEventData EventData = new PointerEventData(EventSystem.current);
            EventData.position = MousePosition;
            EventData.pointerCurrentRaycast = EventData.pointerPressRaycast = result;

            EventData.button = PointerEventData.InputButton.Left;
            EventData.useDragThreshold = false;

            if (SendMouseDown)
            {
                ExecuteEvents.ExecuteHierarchy(result.gameObject, EventData, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.ExecuteHierarchy(result.gameObject, EventData, ExecuteEvents.initializePotentialDrag);
            }

            if (SendMouseDown)
            {
                var dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(result.gameObject);
                if (dragHandler != null && !DragTargets.Contains(dragHandler))
                {
                    DragTargets.Add(dragHandler);
                    ExecuteEvents.Execute(dragHandler, EventData, ExecuteEvents.beginDragHandler);
                }
            }
            else if (IsMouseDown && DragTargets.Count > 0)
            {
                EventData.dragging = true;
                foreach (var t in DragTargets)
                    ExecuteEvents.Execute(t, EventData, ExecuteEvents.dragHandler);
            }


            if (IsMouseDown)
            {
                EventData.button = PointerEventData.InputButton.Left;
            }
            else if (SendMouseUp)
            {
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(result.gameObject, EventData, ExecuteEvents.pointerClickHandler);
            }

            if (hoveredObject != lastHoveredObject)
            {
                if (lastHoveredObject)
                {
                    ExecuteEvents.ExecuteHierarchy(lastHoveredObject, EventData, ExecuteEvents.pointerExitHandler);
                }

                if (hoveredObject != null)
                    ExecuteEvents.ExecuteHierarchy(hoveredObject, EventData, ExecuteEvents.pointerEnterHandler);

                lastHoveredObject = hoveredObject;
            }
        }
        
        //Debug.Log(MousePosition);
    }

    public void Buttontest() => Debug.Log("PRESSED!!");

}
