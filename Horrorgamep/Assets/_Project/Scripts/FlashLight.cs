using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;              // Referință la Spotlight
    [SerializeField] private float maxBattery = 100f;       // Baterie maximă
    [SerializeField] private float batteryDrainRate = 5f;   // Cât de repede scade bateria pe secundă

    private float currentBattery;   // Baterie curentă
    private bool isOn = false;      // Lanterna e aprinsă sau nu

    void Start()
    {
        currentBattery = maxBattery;  // Începem cu baterie plină

        // Găsim spotlight-ul automat dacă nu e setat
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        // Asigurăm că lanterna e stinsă la start
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }

    void Update()
    {
        // Toggle lanterna cu tasta F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        // Dacă lanterna e aprinsă, scade bateria
        if (isOn && currentBattery > 0)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;

            // Dacă bateria ajunge la 0, stinge lanterna
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                isOn = false;
                flashlight.enabled = false;
                Debug.Log("Bateria este epuizată!");
            }
        }
    }

    void ToggleFlashlight()
    {
        // Nu poate aprinde dacă bateria e moartă
        if (currentBattery <= 0)
        {
            Debug.Log("Bateria este epuizată! Nu poți aprinde lanterna.");
            return;
        }

        isOn = !isOn;  // Schimbă starea (on/off)
        flashlight.enabled = isOn;

        if (isOn)
        {
            Debug.Log("Lanterna aprinsă - Baterie: " + currentBattery.ToString("F1") + "%");
        }
        else
        {
            Debug.Log("Lanterna stinsă");
        }
    }

    // Funcții publice ca să putem accesa din alte scripturi (pentru UI mai târziu)
    public float GetBatteryPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }

    public bool IsFlashlightOn()
    {
        return isOn;
    }

    // Funcție pentru a reîncărca bateria (optional, pentru puzzle-uri)
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        Debug.Log("Baterie reîncărcată! Nivel curent: " + currentBattery.ToString("F1") + "%");
    }
}