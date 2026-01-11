using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image batteryBar;
    [SerializeField] private Text puzzleCounter;
    [SerializeField] private GameObject lowStaminaWarning;
    [SerializeField] private GameObject lowBatteryWarning;

    [Header("Warning Thresholds")]
    [SerializeField] private float lowStaminaThreshold = 0.3f;
    [SerializeField] private float lowBatteryThreshold = 0.2f;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1f;

    private PlayerController playerController;
    private LanternSystem lanternSystem;
    private PuzzleManager puzzleManager;

    private bool isFading = false;

    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            lanternSystem = player.GetComponent<LanternSystem>();
        }

        puzzleManager = PuzzleManager.Instance;

        // Hide warnings
        if (lowStaminaWarning != null) lowStaminaWarning.SetActive(false);
        if (lowBatteryWarning != null) lowBatteryWarning.SetActive(false);

        // Start with fade in
        if (fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        UpdateStaminaBar();
        UpdateBatteryBar();
        UpdatePuzzleCounter();
        UpdateWarnings();
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar == null || playerController == null) return;

        float targetFill = playerController.StaminaPercent;
        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, targetFill, Time.deltaTime * 5f);

        // Color based on stamina
        if (targetFill < 0.3f)
            staminaBar.color = Color.red;
        else if (targetFill < 0.6f)
            staminaBar.color = Color.yellow;
        else
            staminaBar.color = Color.green;
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
        // Stamina warning
        if (lowStaminaWarning != null && playerController != null)
        {
            bool showWarning = playerController.StaminaPercent < lowStaminaThreshold;
            lowStaminaWarning.SetActive(showWarning);
        }

        // Battery warning
        if (lowBatteryWarning != null && lanternSystem != null)
        {
            bool showWarning = lanternSystem.BatteryPercent < lowBatteryThreshold && lanternSystem.IsOn;
            lowBatteryWarning.SetActive(showWarning);
        }
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
    [SerializeField] private float pulseSpeed = 2f;

    private bool isOverInteractable = false;

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

        if (Physics.Raycast(ray, out hit, 3f))
        {
            // Check if interactable
            Door door = hit.collider.GetComponent<Door>();
            Collectible collectible = hit.collider.GetComponent<Collectible>();
            KeyPickup key = hit.collider.GetComponent<KeyPickup>();

            isOverInteractable = (door != null || collectible != null || key != null);
        }
        else
        {
            isOverInteractable = false;
        }
    }

    private void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;

        Color targetColor = isOverInteractable ? interactColor : normalColor;

        if (isOverInteractable)
        {
            // Pulse effect
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            targetColor.a = Mathf.Lerp(0.5f, 1f, pulse);
        }

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
    }
}