using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
