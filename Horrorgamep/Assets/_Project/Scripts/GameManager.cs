using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over")]
    [SerializeField] private float restartDelay = 2f;

    [Header("Optional UI")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject hudCanvas;

    private bool isGameOver;

    private void Awake()
    {
        // If there is already a live instance and it's not this one, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // IMPORTANT: keep it across scene reloads (prevents "destroyed instance" issues)
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // If THIS instance is being destroyed, clear Instance so callers don't use a destroyed reference
        if (Instance == this)
            Instance = null;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        if (hudCanvas != null) hudCanvas.SetActive(false);

        StartCoroutine(RestartRoutine());
    }

    public void OnAllPuzzlesComplete()
    {
        // You can change this to Victory UI later
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        if (restartDelay > 0f)
            yield return new WaitForSecondsRealtime(restartDelay);

        // Reset flag before reload so next run works
        isGameOver = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
