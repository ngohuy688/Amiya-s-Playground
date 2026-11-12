using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private string titleSceneName = "Main";

    public static MinigameManager Instance { get; private set; }

    public bool IsRunning { get; private set; } = true;
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void ResetUI()
    {
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
    }

    public void GameOver(int Score)
    {
        ResetUI();
        gameOverMenu.SetActive(true);
        if (!IsRunning)
            return;

        IsRunning = false;
        IsPaused = false;
        IsGameOver = true;

        scoreText.text = "Score: " + Score.ToString();
        Time.timeScale = 0f;
        Debug.Log("Game over. Score: " + Score);
    }

    public void Pause()
    {
        ResetUI();
        pauseMenu.SetActive(true);
        if (!IsRunning)
            return;

        IsRunning = false;
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void Continue()
    {
        ResetUI();
        if (!IsPaused || IsGameOver)
            return;

        IsPaused = false;
        IsRunning = true;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        ResetUI();
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            Debug.LogError("Cannot restart: the active scene is not included in Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void BackToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("Cannot return to title: configure the title scene name in EGameManager.", this);
            return;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            int fileNameStart = scenePath.LastIndexOf('/') + 1;
            int extensionStart = scenePath.LastIndexOf('.');
            string sceneName = scenePath.Substring(fileNameStart, extensionStart - fileNameStart);

            if (sceneName != titleSceneName)
                continue;

            Time.timeScale = 1f;
            IsPaused = false;
            SceneManager.LoadScene(i);
            return;
        }

        Debug.LogError("Title scene '" + titleSceneName + "' is not included in Build Settings.", this);
    }



    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
