using UnityEngine;

public class ComputerUI : Singleton<ComputerUI>
{
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject MainMenuUI;
    [SerializeField] GameObject LobbyUI;
    [SerializeField] GameObject FactionDesignerUI;

    private void Start()
    {
        DeactivateAllUI();
    }
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
}
