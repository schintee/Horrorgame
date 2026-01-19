using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int totalObjectsToCollect = 5;

    [Header("Game Over")]
    [SerializeField] private float restartDelay = 2f;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private UnityEngine.UI.Text timerText;
    [SerializeField] private UnityEngine.UI.Text highScoreText;
    [SerializeField] private UnityEngine.UI.Text objectCountText;

    private bool isGameOver;
    private bool isVictory;
    private float currentTime = 0f;
    private int collectedObjects = 0;
    private float highScore = 0f;

    private const string HIGH_SCORE_KEY = "HighScore";

    public int CollectedObjects => collectedObjects;
    public int TotalObjects => totalObjectsToCollect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadHighScore();
        UpdateUI();

        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (victoryScreen != null) victoryScreen.SetActive(false);
    }

    private void Update()
    {
        if (!isGameOver && !isVictory)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CollectObject()
    {
        collectedObjects++;
        UpdateUI();

        Debug.Log($"[GameManager] Collected {collectedObjects}/{totalObjectsToCollect}");

        if (collectedObjects >= totalObjectsToCollect)
        {
            Victory();
        }
    }

    public void ResetCollectedObjects()
    {
        collectedObjects = 0;
        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver || isVictory) return;
        isGameOver = true;

        Debug.Log("[GameManager] GAME OVER!");

        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        if (hudCanvas != null) hudCanvas.SetActive(false);

        // Close inventory if open
        if (InventorySystem.Instance != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ResetCollectedObjects();
        StartCoroutine(RestartRoutine());
    }

    public void Victory()
    {
        if (isVictory || isGameOver) return;
        isVictory = true;

        Debug.Log("[GameManager] VICTORY!");

        if (highScore == 0f || currentTime < highScore)
        {
            highScore = currentTime;
            SaveHighScore();
            Debug.Log($"[GameManager] NEW HIGH SCORE: {FormatTime(highScore)}");
        }

        if (victoryScreen != null) victoryScreen.SetActive(true);
        if (hudCanvas != null) hudCanvas.SetActive(false);

        if (InventorySystem.Instance != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        if (restartDelay > 0f)
            yield return new WaitForSecondsRealtime(restartDelay);

        isGameOver = false;
        isVictory = false;
        currentTime = 0f;
        collectedObjects = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadSceneByName(string sceneName)
    {
        isGameOver = false;
        isVictory = false;
        currentTime = 0f;
        collectedObjects = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void UpdateUI()
    {
        if (objectCountText != null)
        {
            objectCountText.text = $"Objects: {collectedObjects}/{totalObjectsToCollect}";
        }

        if (highScoreText != null)
        {
            if (highScore > 0f)
                highScoreText.text = $"Best Time: {FormatTime(highScore)}";
            else
                highScoreText.text = "Best Time: --:--";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {FormatTime(currentTime)}";
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetFloat(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
    }
}