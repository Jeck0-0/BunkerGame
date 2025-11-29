using UnityEngine;
using System.Collections;

public class ComputerManager : Singleton<ComputerManager>
{
    [Header("References")]
    [SerializeField] GameObject computer;
    [SerializeField] GameObject screen;
    [SerializeField] Material screenOn;
    [SerializeField] Material screenOff;

    [Header("Animations")]
    [SerializeField] Transform upPosition;
    [SerializeField] Transform downPosition;
    [SerializeField] float slideDuration = 1.2f;
    [SerializeField] AnimationCurve slideUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AnimationCurve slideDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;
    [SerializeField] Transform leftOpenPosition;
    [SerializeField] Transform rightOpenPosition;
    [SerializeField] float openingDuration = 1f;

    [Header("Audio")]
    [SerializeField] AudioClip computerSlidingClip;
    [SerializeField] AudioClip doorsOpeningClip;

    private Coroutine coroutine;
    private bool computerIsUp = false;
    private bool canMoveComputer = true;

    public bool GetComputerUp() => computerIsUp;
    public void BlockComputerMovement(bool block) => canMoveComputer = !block;

    private void Start()
    {
        computer.transform.position = upPosition.position;
        BringComputerDown();
    }
    public void BringComputerUP()
    {
        computerIsUp = true;
        AnimateComputer(true);
    }

    public void BringComputerDown()
    {
        screen.GetComponent<Renderer>().material = screenOff;
        computerIsUp = false;
        AnimateComputer(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canMoveComputer)
        {
            if (!computerIsUp) BringComputerUP();
            else BringComputerDown();
        }
    }

    private void AnimateComputer(bool up)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(Sequence(up));
    }
    private IEnumerator Sequence(bool up)
    {
        canMoveComputer = false;

        if (up)
        {
            yield return StartCoroutine(OpenTableHole(up));
            yield return StartCoroutine(Move(up));
        }
        else
        {
            yield return StartCoroutine(Move(up));
            yield return StartCoroutine(OpenTableHole(up));
        }

        if (up)
        {
            yield return new WaitForSeconds(0.6f);
            screen.GetComponent<Renderer>().material = screenOn;
            ComputerUI.Instance.PauseMenu();
        }

        canMoveComputer = true;
        coroutine = null;
    }

    private IEnumerator OpenTableHole(bool open)
    {
        Vector3 leftStart = leftDoor.transform.position;
        Vector3 rightStart = rightDoor.transform.position;

        Vector3 leftEnd = open ? leftOpenPosition.position : leftDoor.transform.parent.position;
        Vector3 rightEnd = open ? rightOpenPosition.position : rightDoor.transform.parent.position;

        AudioManager.Instance.PlaySound(AudioStorage.Instance.GetComputerDoorsClip(), 0.8f);

        float t = 0f;

        while (t < openingDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / openingDuration);

            leftDoor.transform.position = Vector3.Lerp(leftStart, leftEnd, n);
            rightDoor.transform.position = Vector3.Lerp(rightStart, rightEnd, n);

            yield return null;
        }
    }
    private IEnumerator Move(bool up)
    {
        computer.SetActive(true);
        AudioManager.Instance.PlaySound(AudioStorage.Instance.GetComputerSlidingClip(), 0.6f);

        Vector3 startPos = computer.transform.position;
        Vector3 endPos = up ? upPosition.position : downPosition.position;

        float t = 0f;

        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / slideDuration);
            float curved = up ? slideUpCurve.Evaluate(normalized) : slideDownCurve.Evaluate(normalized);

            computer.transform.position = Vector3.LerpUnclamped(startPos, endPos, curved);
            yield return null;
        }

        computer.transform.position = endPos;

        if (!up) computer.SetActive(false);
    }
}