using UnityEngine;

public class LanternSystem : MonoBehaviour
{
    [Header("Lantern Settings")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject lanternModel;
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainRate = 5f;

    [Header("Light Properties")]
    [SerializeField] private float normalIntensity = 3f;
    [SerializeField] private float flickerIntensity = 1.5f;
    [SerializeField] private float flickerSpeed = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource lanternAudio;
    [SerializeField] private AudioClip toggleOnSound;
    [SerializeField] private AudioClip toggleOffSound;
    [SerializeField] private AudioClip lowBatterySound;

    // State
    private float currentBattery;
    private bool isOn = false;
    private float flickerTimer = 0f;
    private bool hasPlayedLowBatteryWarning = false;

    // Public properties
    public bool IsOn => isOn;
    public float BatteryPercent => currentBattery / maxBattery;

    void Start()
    {
        currentBattery = maxBattery;

        if (lanternLight != null)
            lanternLight.enabled = false;

        if (lanternModel != null)
            lanternModel.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        HandleBattery();
        HandleFlicker();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLantern();
        }
    }

    private void ToggleLantern()
    {
        if (currentBattery <= 0 && !isOn)
        {
            // Can't turn on with no battery
            if (lanternAudio != null && lowBatterySound != null)
            {
                lanternAudio.PlayOneShot(lowBatterySound);
            }
            return;
        }

        isOn = !isOn;

        if (lanternLight != null)
            lanternLight.enabled = isOn;

        if (lanternModel != null)
            lanternModel.SetActive(isOn);

        // Play toggle sound
        if (lanternAudio != null)
        {
            if (isOn && toggleOnSound != null)
                lanternAudio.PlayOneShot(toggleOnSound);
            else if (!isOn && toggleOffSound != null)
                lanternAudio.PlayOneShot(toggleOffSound);
        }
    }

    private void HandleBattery()
    {
        if (!isOn || currentBattery <= 0) return;

        currentBattery -= batteryDrainRate * Time.deltaTime;
        currentBattery = Mathf.Max(currentBattery, 0);

        // Check for low battery
        if (currentBattery <= 20f && !hasPlayedLowBatteryWarning)
        {
            hasPlayedLowBatteryWarning = true;
            if (lanternAudio != null && lowBatterySound != null)
            {
                lanternAudio.PlayOneShot(lowBatterySound);
            }
        }

        // Turn off when battery runs out
        if (currentBattery <= 0 && isOn)
        {
            isOn = false;
            if (lanternLight != null)
                lanternLight.enabled = false;
            if (lanternModel != null)
                lanternModel.SetActive(false);
        }
    }

    private void HandleFlicker()
    {
        if (!isOn || lanternLight == null) return;

        // Flicker more when battery is low
        float flickerChance = Mathf.Lerp(0.05f, 0.3f, 1f - (currentBattery / maxBattery));

        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0)
        {
            if (Random.value < flickerChance)
            {
                // Flicker
                lanternLight.intensity = Random.Range(flickerIntensity, normalIntensity);
            }
            else
            {
                lanternLight.intensity = normalIntensity;
            }

            flickerTimer = flickerSpeed;
        }
    }

    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        hasPlayedLowBatteryWarning = false;
    }

    public void ForceToggle(bool state)
    {
        isOn = state;
        if (lanternLight != null)
            lanternLight.enabled = isOn;
        if (lanternModel != null)
            lanternModel.SetActive(isOn);
    }
}