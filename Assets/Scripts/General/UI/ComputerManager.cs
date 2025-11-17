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
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine coroutine;
    private bool computerIsUp = false;
    private bool canMoveComputer = true;

    public bool GetComputerUp() => computerIsUp;
    public void BlockComputerMovement(bool block) => canMoveComputer = !block;

    private void Start()
    {
        computer.transform.position = downPosition.position;
        computer.SetActive(false);
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
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(Move(up));
    }
    private IEnumerator Move(bool up)
    {
        computer.SetActive(true);

        Vector3 startPos = computer.transform.position;
        Vector3 endPos = up ? upPosition.position : downPosition.position;

        float t = 0f;

        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / slideDuration);
            float curved = slideCurve.Evaluate(normalized);

            computer.transform.position = Vector3.LerpUnclamped(startPos, endPos, curved);
            yield return null;
        }

        computer.transform.position = endPos;

        if (!up) computer.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        if (up)
        {
            screen.GetComponent<Renderer>().material = screenOn;
            ComputerUI.Instance.PauseMenu();
        }

        coroutine = null;
    }
}