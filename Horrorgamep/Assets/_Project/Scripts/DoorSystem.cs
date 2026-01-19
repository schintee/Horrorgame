using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string requiredKey = "";

    [Header("Collision Handling (IMPORTANT)")]
    [Tooltip("Layer-ul playerului (ex: Player). Folosit pentru ignorarea coliziunilor.")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("Daca e TRUE, cand usa e deschisa se ignora TOATE coliziunile cu playerul.")]
    [SerializeField] private bool ignorePlayerCollisionWhenOpen = true;

    [Header("Enemy Rules")]
    [SerializeField] private bool enemyAlwaysPassThrough = true;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Level Progression")]
    [SerializeField] private bool isExitDoor = false;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private float loadDelay = 0.5f;

    [Header("Animation")]
    [SerializeField] private bool slideOpen = false;
    [SerializeField] private Vector3 openPosition = Vector3.zero;
    [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0);
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource doorAudio;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool isOpen = false;
    private bool isMoving = false;

    private Vector3 closedPosition;
    private Quaternion closedRotation;

    private Coroutine autoCloseCoroutine;
    private Coroutine loadNextCoroutine;

    private Collider[] doorColliders;
    private Collider[] playerColliders;

    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;

    private string Normalize(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? "" : id.Trim().ToLowerInvariant();
    }

    private string NiceKeyName(string id)
    {
        id = Normalize(id);
        switch (id)
        {
            case "bluekey": return "CHEIA ALBASTRA";
            case "redkey": return "CHEIA ROSIE";
            case "greykey":
            case "graykey": return "CHEIA GRI";
            default: return id.ToUpperInvariant();
        }
    }

    private void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        doorColliders = GetComponentsInChildren<Collider>(true);

        CachePlayerColliders();

        if (enemyAlwaysPassThrough)
        {
            if (enemyLayerMask.value == 0)
            {
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                if (enemyLayer >= 0)
                    enemyLayerMask = 1 << enemyLayer;
            }

            SetupEnemyPassThrough();
        }
    }

    private void Start()
    {
        closedPosition = doorTransform.localPosition;
        closedRotation = doorTransform.localRotation;

        ApplyCollisionIgnoreState();

        if (debugMode)
        {
            Debug.Log($"[Door] {gameObject.name} initialized. Locked: {isLocked}, Required Key: '{requiredKey}'", this);
        }
    }

    private void CachePlayerColliders()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Door] Player with tag 'Player' not found!");
            return;
        }

        playerColliders = player.GetComponentsInChildren<Collider>(true);
    }

    public void Interact()
    {
        if (isMoving) return;

        if (isLocked)
        {
            string req = Normalize(requiredKey);

            if (!string.IsNullOrEmpty(req))
            {
                if (!TryUnlock(req))
                {
                    string nice = NiceKeyName(req);
                    Debug.Log($"[Door] USA E BLOCATA! Ai nevoie de: {nice}", this);

                    if (InventorySystem.Instance != null)
                        InventorySystem.Instance.ShowWrongKeyMessage(nice);

                    PlayLockedSound();
                    return;
                }
            }
            else
            {
                Debug.Log("[Door] USA E BLOCATA (fără cheie specifică)", this);
                PlayLockedSound();
                return;
            }
        }

        if (isOpen) Close();
        else Open();
    }

    private bool TryUnlock(string normalizedRequiredKey)
    {
        // Metodă 1: Caută în toată scena
        PlayerInventory[] allInventories = FindObjectsOfType<PlayerInventory>();

        if (debugMode)
        {
            Debug.Log($"[Door] Găsite {allInventories.Length} PlayerInventory în scenă", this);
        }

        foreach (PlayerInventory inv in allInventories)
        {
            if (inv != null)
            {
                bool hasKey = inv.HasKey(normalizedRequiredKey);

                if (debugMode)
                {
                    Debug.Log($"[Door] Verific PlayerInventory pe '{inv.gameObject.name}' pentru cheia '{normalizedRequiredKey}' - Găsită: {hasKey}", this);
                }

                if (hasKey)
                {
                    Debug.Log($"[Door] ✓ Cheia '{normalizedRequiredKey}' GĂSITĂ! Deschid ușa!", this);
                    Unlock();
                    return true;
                }
            }
        }

        // Metodă 2: Caută prin InventorySystem (UI)
        if (InventorySystem.Instance != null)
        {
            bool hasInUI = InventorySystem.Instance.HasItem(normalizedRequiredKey);
            if (debugMode)
            {
                Debug.Log($"[Door] Verific InventorySystem UI pentru '{normalizedRequiredKey}' - Găsită: {hasInUI}", this);
            }

            if (hasInUI)
            {
                Debug.Log($"[Door] ✓ Cheia '{normalizedRequiredKey}' găsită în UI! Deschid ușa!", this);
                Unlock();
                return true;
            }
        }

        Debug.Log($"[Door] ✗ Cheia '{normalizedRequiredKey}' NU a fost găsită nicăieri!", this);
        return false;
    }

    public void Unlock() => isLocked = false;

    public void Open()
    {
        if (isOpen || isMoving) return;

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        StartCoroutine(MoveDoor(true));
    }

    public void Close()
    {
        if (!isOpen || isMoving) return;

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        StartCoroutine(MoveDoor(false));
    }

    private IEnumerator MoveDoor(bool opening)
    {
        isMoving = true;

        if (doorAudio != null)
        {
            AudioClip clip = opening ? openSound : closeSound;
            if (clip != null) doorAudio.PlayOneShot(clip);
        }

        float t = 0f;
        Vector3 startPos = doorTransform.localPosition;
        Quaternion startRot = doorTransform.localRotation;

        Vector3 targetPos = opening ? (closedPosition + openPosition) : closedPosition;
        Quaternion targetRot = opening ? (closedRotation * Quaternion.Euler(openRotation)) : closedRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            if (slideOpen)
                doorTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            else
                doorTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        isOpen = opening;
        isMoving = false;

        ApplyCollisionIgnoreState();

        if (autoClose && isOpen && !isExitDoor)
            autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());

        if (opening && isExitDoor && !string.IsNullOrEmpty(nextSceneName))
            loadNextCoroutine = StartCoroutine(LoadNextAfterDelay());
    }

    private void ApplyCollisionIgnoreState()
    {
        if (!ignorePlayerCollisionWhenOpen) return;
        if (playerColliders == null || doorColliders == null) return;

        foreach (Collider doorCol in doorColliders)
        {
            if (doorCol == null) continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol == null) continue;

                Physics.IgnoreCollision(doorCol, playerCol, isOpen);
            }
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        Close();
    }

    private IEnumerator LoadNextAfterDelay()
    {
        yield return new WaitForSeconds(loadDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneByName(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }

    private void PlayLockedSound()
    {
        if (doorAudio != null && lockedSound != null)
            doorAudio.PlayOneShot(lockedSound);
    }

    private void SetupEnemyPassThrough()
    {
        Collider[] all = FindObjectsOfType<Collider>(true);

        foreach (Collider c in all)
        {
            if (c == null) continue;
            if (((1 << c.gameObject.layer) & enemyLayerMask.value) == 0) continue;

            foreach (Collider doorCol in doorColliders)
            {
                if (doorCol == null) continue;
                Physics.IgnoreCollision(doorCol, c, true);
            }
        }
    }
}