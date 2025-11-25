using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float transitionDuration;

    private static SceneLoader _instance;

    #region Singleton
    public static SceneLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SceneLoader>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SceneLoader");
                    _instance = singletonObject.AddComponent<SceneLoader>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion

    private void Start()
    {
        if (obj)
        obj.SetActive(true);
    }
    public void LoadScene(string name)
    {
        StartCoroutine(TransitionToScene(name));
    }
    public void LoadNextScene()
    {
        StartCoroutine(TransitionToScene("", SceneManager.GetActiveScene().buildIndex +1));
    }
    public void RestartScene()
    {
        StartCoroutine(TransitionToScene(SceneManager.GetActiveScene().name));
    }
    IEnumerator TransitionToScene(string name = "", int buildIndex = -1)
    {
        if (canvasGroup) yield return StartCoroutine(FadeCanvas(1f));

        yield return new WaitForSeconds(1);

        if (name != "")
        SceneManager.LoadScene(name);
        else if (buildIndex >= 0)
        SceneManager.LoadScene(buildIndex);

        // Wait until the new scene is fully loaded
        yield return null;
        yield return new WaitForSeconds(0.1f);
        Cursor.visible = true;

        if (canvasGroup) yield return StartCoroutine(FadeCanvas(0f));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / transitionDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}