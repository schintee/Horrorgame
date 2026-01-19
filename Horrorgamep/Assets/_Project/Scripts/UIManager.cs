using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private Image batteryBar;
    [SerializeField] private Text puzzleCounter;
    [SerializeField] private GameObject lowBatteryWarning;

    [Header("Enemy Health Bar")]
    [SerializeField] private GameObject enemyHealthPanel;
    [SerializeField] private Image enemyHealthBar;
    [SerializeField] private Text enemyNameText;

    [Header("Warning Thresholds")]
    [SerializeField] private float lowBatteryThreshold = 0.2f;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1f;

    private LanternSystem lanternSystem;
    private PuzzleManager puzzleManager;
    private EnemyHealth currentEnemy;

    private bool isFading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            lanternSystem = player.GetComponent<LanternSystem>();
        }

        puzzleManager = PuzzleManager.Instance;

        // Hide warnings
        if (lowBatteryWarning != null) lowBatteryWarning.SetActive(false);
        if (enemyHealthPanel != null) enemyHealthPanel.SetActive(false);

        // Start with fade in
        if (fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        UpdateBatteryBar();
        UpdatePuzzleCounter();
        UpdateWarnings();
        UpdateEnemyHealthBar();
    }

    private void UpdateBatteryBar()
    {
        if (batteryBar == null || lanternSystem == null) return;

        float targetFill = lanternSystem.BatteryPercent;
        batteryBar.fillAmount = Mathf.Lerp(batteryBar.fillAmount, targetFill, Time.deltaTime * 5f);

        // Color based on battery
        if (targetFill < 0.2f)
            batteryBar.color = Color.red;
        else if (targetFill < 0.5f)
            batteryBar.color = Color.yellow;
        else
            batteryBar.color = Color.cyan;
    }

    private void UpdatePuzzleCounter()
    {
        if (puzzleCounter == null || puzzleManager == null) return;

        puzzleCounter.text = $"Puzzles: {puzzleManager.CompletedPuzzlesCount}/{puzzleManager.TotalPuzzles}";
    }

    private void UpdateWarnings()
    {
        // Battery warning
        if (lowBatteryWarning != null && lanternSystem != null)
        {
            bool showWarning = lanternSystem.BatteryPercent < lowBatteryThreshold && lanternSystem.IsOn;
            lowBatteryWarning.SetActive(showWarning);
        }
    }

    // === ENEMY HEALTH BAR ===
    public void ShowEnemyHealth(EnemyHealth enemy)
    {
        if (enemyHealthPanel == null) return;

        currentEnemy = enemy;
        enemyHealthPanel.SetActive(true);

        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.gameObject.name;
        }
    }

    public void HideEnemyHealth()
    {
        if (enemyHealthPanel != null)
        {
            enemyHealthPanel.SetActive(false);
        }
        currentEnemy = null;
    }

    private void UpdateEnemyHealthBar()
    {
        if (currentEnemy == null || enemyHealthBar == null) return;

        float targetFill = currentEnemy.GetHealthPercent();
        enemyHealthBar.fillAmount = Mathf.Lerp(enemyHealthBar.fillAmount, targetFill, Time.deltaTime * 10f);

        // Color based on health
        if (targetFill < 0.3f)
            enemyHealthBar.color = Color.red;
        else if (targetFill < 0.6f)
            enemyHealthBar.color = Color.yellow;
        else
            enemyHealthBar.color = Color.green;
    }

    // Fade effects
    public System.Collections.IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        isFading = true;
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        isFading = false;
    }

    public System.Collections.IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        isFading = true;
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        while (color.a < 1)
        {
            color.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }

        isFading = false;
    }

    // Notification system
    [Header("Notifications")]
    [SerializeField] private Text notificationText;
    [SerializeField] private float notificationDuration = 3f;

    private float notificationTimer = 0f;

    public void ShowNotification(string message)
    {
        if (notificationText == null) return;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        notificationTimer = notificationDuration;

        StopAllCoroutines();
        StartCoroutine(HideNotificationAfterDelay());
    }

    private System.Collections.IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }
}

// Crosshair system
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private float pulseSpeed = 2f;

    private bool isOverInteractable = false;
    private bool isOverEnemy = false;

    void Update()
    {
        UpdateCrosshairColor();
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            // Check enemy
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                isOverEnemy = true;
                isOverInteractable = false;
                return;
            }

            // Check if interactable
            Door door = hit.collider.GetComponentInParent<Door>();
            CollectibleItem collectible = hit.collider.GetComponent<CollectibleItem>();
            if (collectible == null) collectible = hit.collider.GetComponentInParent<CollectibleItem>();
            KeyPickup key = hit.collider.GetComponent<KeyPickup>();

            isOverInteractable = (door != null || collectible != null || key != null);
            isOverEnemy = false;
        }
        else
        {
            isOverInteractable = false;
            isOverEnemy = false;
        }
    }

    private void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;

        Color targetColor = normalColor;

        if (isOverEnemy)
        {
            targetColor = enemyColor;
        }
        else if (isOverInteractable)
        {
            targetColor = interactColor;
        }

        if (isOverInteractable || isOverEnemy)
        {
            // Pulse effect
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            targetColor.a = Mathf.Lerp(0.5f, 1f, pulse);
        }

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
    }
}