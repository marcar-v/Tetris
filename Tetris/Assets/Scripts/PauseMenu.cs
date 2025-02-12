using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    public Piece piece;
    public static bool isPaused;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }
    public void PausePanel()
    {
        pauseMenu.SetActive(true);
        audioManager.PlaySFX(audioManager.clickSound);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        audioManager.PlaySFX(audioManager.clickSound);
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        audioManager.PlaySFX(audioManager.clickSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnMainMenu()
    {
        audioManager.PlaySFX(audioManager.clickSound);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        audioManager.PlaySFX(audioManager.clickSound);
        Application.Quit();
    }
}
