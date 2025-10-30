using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject FactionDesigner;
    void Start()
    {
        mainMenu.SetActive(true);
        FactionDesigner.SetActive(false);
    }

    void Update()
    {

    }
    public void StartGame()
    {

    }

    public void FactionDesigning()
    {
        mainMenu.SetActive(false);
        FactionDesigner.SetActive(true);
    }

    public void BacktoMenu()
    {
        mainMenu.SetActive(true);
        FactionDesigner.SetActive(false);
    }
    public void Options()
    {
        // MYBE?
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
