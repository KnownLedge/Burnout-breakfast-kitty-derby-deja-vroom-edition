using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject regularMainMenuButtons;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    public GameObject pauseMenuPanel;
    [SerializeField] private GameObject winScreenPanel;

    void Start()
    {
        Time.timeScale = 1; //Ensure the game is unpaused at the start of every scene.
    }

    public void MainMenuStart()
    {
        SceneManager.LoadScene("DanielTestScene");
    }

    public void MainMenuSettings()
    {
        settingsPanel.SetActive(true);
        regularMainMenuButtons.SetActive(false);
    }

    public void SettingsMenuBackToMainMenu()
    {
        settingsPanel.SetActive(false);
        regularMainMenuButtons.SetActive(true);
    }

    public void MainMenuCredits()
    {
        creditsPanel.SetActive(true);
        regularMainMenuButtons.SetActive(false);
    }

    public void CreditsMenuBackToMainMenu()
    {
        creditsPanel.SetActive(false);
        regularMainMenuButtons.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PauseMenuToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseMenuResume()
    {
        Time.timeScale = 1; //Unpause time.
        pauseMenuPanel.SetActive(false);
    }

    public void PauseMenuSettings()
    {
        settingsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void SettingsMenuBackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void PauseMenuCredits()
    {
        creditsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    public void CreditsMenuBackToPauseMenu()
    {
        creditsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void ShowWinScreen()
    {
        Time.timeScale = 0; //Pause time.
        winScreenPanel.SetActive(true);
    }

    public void WinScreenBackToGame()
    {
        Time.timeScale = 1; //Unpause time.
        winScreenPanel.SetActive(false);
    }
}
