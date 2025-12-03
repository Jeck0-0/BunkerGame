using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerUI : Singleton<ComputerUI>
{
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject MainMenuUI;
    [SerializeField] GameObject LobbyUI;
    [SerializeField] GameObject FactionDesignerUI;

    public void PauseMenu()
    {
        DeactivateAllUI();
        pauseMenuUI.SetActive(true);
    }
    public void MainMenu()
    {
        DeactivateAllUI();
        MainMenuUI.SetActive(true);
    }
    public void Lobby()
    {
        DeactivateAllUI();
        LobbyUI.SetActive(true);
    }
    public void FactionDesigner()
    {
        DeactivateAllUI();
        FactionDesignerUI.SetActive(true);
    }
    public void TurnComputerOff()
    {
        DeactivateAllUI();
        ComputerManager.Instance.BringComputerDown();
    }
    public void DeactivateAllUI()
    {
        pauseMenuUI.SetActive(false);
        MainMenuUI.SetActive(false);
        LobbyUI.SetActive(false);
        FactionDesignerUI.SetActive(false);
    }
    public void GoToMainMenu()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("MainMenu");
            return;
        }
        SceneManager.LoadScene("MainMenu");
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
