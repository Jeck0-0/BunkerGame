using UnityEngine;

public class PlayerInteractor : MonoBehaviour // for testing
{
    [SerializeField] Camera playerCamera;

    private TapePlayerButton hoveredButton;
    private TapeObject hoveredTape;
    private YourNamePlate hoveredPlate;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }
    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 99f))
        {
            TapePlayerButton button = hit.collider.GetComponent<TapePlayerButton>();
            TapeObject tape = hit.collider.GetComponent<TapeObject>();
            YourNamePlate plate = hit.collider.GetComponent <YourNamePlate>();

            if (button != null)
            {
                HandleButtonHover(button);

                if (Input.GetMouseButtonDown(0))
                button.OnPressed();
            }
            else if (tape != null)
            {
                HandleTapeHover(tape);

                if (Input.GetMouseButtonDown(0))
                tape.InsertTape();
            }
            else if (plate != null)
            {
                HandlePlateHover(plate);

                if(Input.GetMouseButtonDown(0))
                    plate.ShowObjective();
            }
            else
            {
                ClearHover();
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void HandleButtonHover(TapePlayerButton button)
    {
        if (hoveredButton != button)
        {
            hoveredButton?.OnHoverExit();
            hoveredButton = button;
            hoveredButton.OnHoverEnter();
        }

        if (hoveredTape != null)
        {
            hoveredTape.OnHoverExit();
            hoveredTape = null;
        }

        if (hoveredPlate != null)
        {
            hoveredPlate.OnHoverExit();
            hoveredPlate = null;
        }
    }

    private void HandleTapeHover(TapeObject tape)
    {
        if (hoveredTape != tape)
        {
            hoveredTape?.OnHoverExit();
            hoveredTape = tape;
            hoveredTape.OnHoverEnter();
        }

        if (hoveredButton != null)
        {
            hoveredButton.OnHoverExit();
            hoveredButton = null;
        }

        if (hoveredPlate != null)
        {
            hoveredPlate.OnHoverExit();
            hoveredPlate = null;
        }
    }

    private void HandlePlateHover(YourNamePlate plate)
    {
        if (hoveredPlate != plate)
        {
            hoveredPlate?.OnHoverExit();
            hoveredPlate = plate;
            hoveredPlate.OnHoverEnter();
        }

        if (hoveredButton != null)
        {
            hoveredButton.OnHoverExit();
            hoveredButton = null;
        }

        if (hoveredTape != null)
        {
            hoveredTape.OnHoverExit();
            hoveredTape = null;
        }
    }

    private void ClearHover()
    {
        if (hoveredButton != null)
        {
            hoveredButton.OnHoverExit();
            hoveredButton = null;
        }

        if (hoveredTape != null)
        {
            hoveredTape.OnHoverExit();
            hoveredTape = null;
        }

        if (hoveredPlate != null)
        {
            hoveredPlate.OnHoverExit();
            hoveredPlate = null;
        }
    }
}