using UnityEngine;
using System.Collections.Generic;

public class SafeZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private bool isActive = true;

    [Header("Visual Feedback")]
    [SerializeField] private Light safeLight;
    [SerializeField] private ParticleSystem safeParticles;
    [SerializeField] private Color lightColor = Color.cyan;

    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;

    [Header("Restoration")]
    [SerializeField] private bool restoreBattery = true;
    [SerializeField] private float batteryRestoreRate = 15f;

    // State
    private HashSet<GameObject> entitiesInZone = new HashSet<GameObject>();
    private PlayerController playerInZone;
    private LanternSystem lanternInZone;

    // Static tracking
    private static HashSet<SafeZone> allSafeZones = new HashSet<SafeZone>();

    public static bool IsPlayerInAnySafeZone { get; private set; }
    public bool IsPlayerInThisZone => playerInZone != null;
    public bool IsActive => isActive;

    void OnEnable()
    {
        allSafeZones.Add(this);
    }

    void OnDisable()
    {
        allSafeZones.Remove(this);
    }

    void Start()
    {
        SetupVisuals();
    }

    void Update()
    {
        if (!isActive || playerInZone == null) return;

        RestorePlayerResources();
    }

    private void SetupVisuals()
    {
        if (safeLight != null)
        {
            safeLight.color = lightColor;
            safeLight.enabled = isActive;
        }

        if (safeParticles != null)
        {
            if (isActive)
                safeParticles.Play();
            else
                safeParticles.Stop();
        }
    }

    private void RestorePlayerResources()
    {
        // Doar Battery restore (stamina a fost eliminată)
        if (restoreBattery && lanternInZone != null)
        {
            lanternInZone.AddBattery(batteryRestoreRate * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        entitiesInZone.Add(other.gameObject);

        // Check if player
        if (other.CompareTag("Player"))
        {
            playerInZone = other.GetComponent<PlayerController>();
            lanternInZone = other.GetComponent<LanternSystem>();

            UpdateGlobalPlayerStatus();

            // Play enter sound
            if (ambientAudio != null && enterSound != null)
            {
                ambientAudio.PlayOneShot(enterSound);
            }

            OnPlayerEnter();
        }
    }

    void OnTriggerExit(Collider other)
    {
        entitiesInZone.Remove(other.gameObject);

        // Check if player
        if (other.CompareTag("Player"))
        {
            playerInZone = null;
            lanternInZone = null;

            UpdateGlobalPlayerStatus();

            // Play exit sound
            if (ambientAudio != null && exitSound != null)
            {
                ambientAudio.PlayOneShot(exitSound);
            }

            OnPlayerExit();
        }
    }

    private void UpdateGlobalPlayerStatus()
    {
        IsPlayerInAnySafeZone = false;

        foreach (var zone in allSafeZones)
        {
            if (zone.IsPlayerInThisZone && zone.IsActive)
            {
                IsPlayerInAnySafeZone = true;
                break;
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        SetupVisuals();

        if (!isActive && playerInZone != null)
        {
            UpdateGlobalPlayerStatus();
        }
    }

    public bool IsEntityInZone(GameObject entity)
    {
        return isActive && entitiesInZone.Contains(entity);
    }

    // Check if a position is within the safe zone
    public bool IsPositionInZone(Vector3 position)
    {
        if (!isActive) return false;
        return Vector3.Distance(transform.position, position) <= radius;
    }

    // Virtual methods for custom behavior
    protected virtual void OnPlayerEnter()
    {
        // Override in derived classes for custom behavior
        Debug.Log("Player entered safe zone");
    }

    protected virtual void OnPlayerExit()
    {
        // Override in derived classes for custom behavior
        Debug.Log("Player exited safe zone");
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = isActive ? safeColor : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw safe zone indicator
        Gizmos.color = new Color(safeColor.r, safeColor.g, safeColor.b, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    // Static helper method
    public static SafeZone GetNearestSafeZone(Vector3 position, float maxDistance = Mathf.Infinity)
    {
        SafeZone nearest = null;
        float minDistance = maxDistance;

        foreach (var zone in allSafeZones)
        {
            if (!zone.IsActive) continue;

            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = zone;
            }
        }

        return nearest;
    }
}