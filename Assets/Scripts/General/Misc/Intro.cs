using System.Collections;
using TMPro;
using UnityEngine;

public class Intro : MonoBehaviour
{
    [SerializeField] GameObject typingText;
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField, TextArea] string text;

    [SerializeField] GameObject revealText;
    [SerializeField] AudioClip revealSound;
    [SerializeField] AudioClip introAmbient;

    [Header("Typewriter Settings")]
    [SerializeField] AudioClip[] typeSounds;
    [SerializeField] AudioClip[] nextPageSounds;
    [SerializeField] float minCharDelay = 0.03f;
    [SerializeField] float maxCharDelay = 0.08f;
    [SerializeField] float punctuationPause = 0.25f;


    void Awake()
    {
        revealText.SetActive(false);
        typingText.SetActive(true);
        if (introAmbient) AudioManager.Instance.PlaySound(introAmbient, 1f);

        StartCoroutine(TypeText());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            SceneLoader.Instance.LoadNextScene();
        }
    }

    private IEnumerator TypeText()
    {
        textField.text = "";
        yield return new WaitForSeconds(1f);

        foreach (char c in text)
        {
            textField.text += c;

            float delay = Random.Range(minCharDelay, maxCharDelay);

            if (c == '\n')
            {
                if (nextPageSounds.Length > 0) AudioManager.Instance.PlayRandomSound(nextPageSounds, 1f);
                delay += punctuationPause;
            }
            else
            {
                if (typeSounds.Length > 0) AudioManager.Instance.PlayRandomSound(typeSounds, Random.Range(0.4f, 0.6f));
            }

            if (".,!?…".Contains(c)) delay += punctuationPause;
            if (" ".Contains(c)) delay = 0f;

            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(1.9f);
        if (revealSound) AudioManager.Instance.PlaySound(revealSound);
        yield return new WaitForSeconds(0.1f);

        revealText.SetActive(true);
        typingText.SetActive(false);

        yield return new WaitForSeconds(3f);
        SceneLoader.Instance.LoadNextScene();
    }
}
