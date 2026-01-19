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
    [SerializeField] private bool ignorePlayerCollisionWhenOpen = false; // SCHIMBAT LA FALSE BY DEFAULT

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

        Debug.Log($"[Door '{gameObject.name}'] Găsite {doorColliders.Length} collidere", this);

        // Verifică dacă sunt trigger
        foreach (var col in doorColliders)
        {
            if (col != null)
            {
                Debug.Log($"[Door '{gameObject.name}'] Collider '{col.name}': IsTrigger={col.isTrigger}", this);

                // IMPORTANT: Asigură-te că nu e trigger!
                if (col.isTrigger)
                {
                    Debug.LogWarning($"[Door '{gameObject.name}'] ATENȚIE: Collider-ul '{col.name}' este TRIGGER! Ar trebui să fie normal collider!", this);
                }
            }
        }

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

        Debug.Log($"[Door '{gameObject.name}'] === CONFIGURARE ===", this);
        Debug.Log($"  - IsLocked: {isLocked}", this);
        Debug.Log($"  - RequiredKey: '{requiredKey}'", this);
        Debug.Log($"  - RequiredKey (normalized): '{Normalize(requiredKey)}'", this);
        Debug.Log($"  - SlideOpen: {slideOpen}", this);
        Debug.Log($"  - OpenSpeed: {openSpeed}", this);
        Debug.Log($"  - IgnorePlayerCollision: {ignorePlayerCollisionWhenOpen}", this);
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
        Debug.Log($"[Door '{gameObject.name}'] Găsite {playerColliders.Length} collidere pe Player", this);
    }

    public void Interact()
    {
        Debug.Log($"[Door '{gameObject.name}'] ========== INTERACT APELAT ==========", this);
        Debug.Log($"  - isMoving: {isMoving}", this);
        Debug.Log($"  - isOpen: {isOpen}", this);
        Debug.Log($"  - isLocked: {isLocked}", this);

        if (isMoving)
        {
            Debug.Log("[Door] BLOCAT: Usa se mișcă deja!", this);
            return;
        }

        // Dacă e deschisă, închide-o
        if (isOpen)
        {
            Debug.Log("[Door] Usa e deschisă → o închid", this);
            Close();
            return;
        }

        // Dacă e blocată, verifică cheia
        if (isLocked)
        {
            Debug.Log("[Door] Usa e LOCKED, verific cheia...", this);

            string req = Normalize(requiredKey);
            Debug.Log($"  - Required key (normalized): '{req}'", this);

            if (string.IsNullOrEmpty(req))
            {
                Debug.Log("[Door] BLOCAT: Usa e locked permanent (fără cheie)", this);
                PlayLockedSound();
                return;
            }

            // Verifică dacă playerul are cheia necesară
            if (!HasRequiredKey(req))
            {
                string nice = NiceKeyName(req);
                Debug.Log($"[Door] BLOCAT: ✗ NU ai cheia '{req}'!", this);

                if (InventorySystem.Instance != null)
                    InventorySystem.Instance.ShowWrongKeyMessage(nice);

                PlayLockedSound();
                return;
            }

            // Are cheia! Deschide usa
            Debug.Log($"[Door] ✓✓✓ AI CHEIA '{req}'! Unlock și deschid usa!", this);
            isLocked = false; // Unlock
        }

        // Deschide usa
        Debug.Log("[Door] → Apelez Open()", this);
        Open();
    }

    private bool HasRequiredKey(string normalizedRequiredKey)
    {
        Debug.Log($"[Door '{gameObject.name}'] === VERIFIC CHEIA '{normalizedRequiredKey}' ===", this);

        if (string.IsNullOrEmpty(normalizedRequiredKey))
        {
            Debug.Log("[Door] RequiredKey este gol → FALSE", this);
            return false;
        }

        // Caută PlayerInventory
        PlayerInventory[] allInventories = FindObjectsOfType<PlayerInventory>();
        Debug.Log($"[Door] Găsite {allInventories.Length} PlayerInventory în scenă", this);

        foreach (PlayerInventory inv in allInventories)
        {
            if (inv != null)
            {
                Debug.Log($"[Door] Verific inventar pe '{inv.gameObject.name}'...", this);

                if (inv.HasKey(normalizedRequiredKey))
                {
                    Debug.Log($"[Door] ✓✓✓ CHEIA '{normalizedRequiredKey}' GĂSITĂ!", this);
                    return true;
                }
            }
        }

        // Verifică și în UI inventory
        if (InventorySystem.Instance != null)
        {
            Debug.Log("[Door] Verific și în InventorySystem UI...", this);

            if (InventorySystem.Instance.HasItem(normalizedRequiredKey))
            {
                Debug.Log($"[Door] ✓✓✓ CHEIA '{normalizedRequiredKey}' GĂSITĂ în UI!", this);
                return true;
            }
        }

        Debug.Log($"[Door] ✗✗✗ CHEIA '{normalizedRequiredKey}' NU EXISTĂ!", this);
        return false;
    }

    public void Unlock()
    {
        isLocked = false;
        Debug.Log("[Door] Unlock() apelat manual", this);
    }

    public void Open()
    {
        Debug.Log($"[Door '{gameObject.name}'] Open() apelat. isOpen={isOpen}, isMoving={isMoving}", this);

        if (isOpen)
        {
            Debug.Log("[Door] Deja deschisă, skip", this);
            return;
        }

        if (isMoving)
        {
            Debug.Log("[Door] Se mișcă deja, skip", this);
            return;
        }

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        Debug.Log("[Door] → StartCoroutine(MoveDoor(true))", this);
        StartCoroutine(MoveDoor(true));
    }

    public void Close()
    {
        Debug.Log($"[Door '{gameObject.name}'] Close() apelat", this);

        if (!isOpen || isMoving) return;

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        StartCoroutine(MoveDoor(false));
    }

    private IEnumerator MoveDoor(bool opening)
    {
        isMoving = true;

        Debug.Log($"[Door '{gameObject.name}'] ►►► ÎNCEP ANIMAȚIE: {(opening ? "OPEN" : "CLOSE")} ◄◄◄", this);

        if (doorAudio != null)
        {
            AudioClip clip = opening ? openSound : closeSound;
            if (clip != null)
            {
                doorAudio.PlayOneShot(clip);
                Debug.Log("[Door] Sunet redat", this);
            }
        }

        float t = 0f;
        Vector3 startPos = doorTransform.localPosition;
        Quaternion startRot = doorTransform.localRotation;

        Vector3 targetPos = opening ? (closedPosition + openPosition) : closedPosition;
        Quaternion targetRot = opening ? (closedRotation * Quaternion.Euler(openRotation)) : closedRotation;

        Debug.Log($"[Door] Start Pos: {startPos}, Target Pos: {targetPos}", this);
        Debug.Log($"[Door] Start Rot: {startRot.eulerAngles}, Target Rot: {targetRot.eulerAngles}", this);
        Debug.Log($"[Door] SlideOpen: {slideOpen}", this);

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            if (slideOpen)
            {
                doorTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            }
            else
            {
                doorTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            }

            yield return null;
        }

        // Finalizare
        if (slideOpen)
            doorTransform.localPosition = targetPos;
        else
            doorTransform.localRotation = targetRot;

        isOpen = opening;
        isMoving = false;

        Debug.Log($"[Door '{gameObject.name}'] ►►► ANIMAȚIE FINALIZATĂ! isOpen={isOpen} ◄◄◄", this);

        ApplyCollisionIgnoreState();

        if (autoClose && isOpen && !isExitDoor)
            autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());

        if (opening && isExitDoor && !string.IsNullOrEmpty(nextSceneName))
            loadNextCoroutine = StartCoroutine(LoadNextAfterDelay());
    }

    private void ApplyCollisionIgnoreState()
    {
        if (!ignorePlayerCollisionWhenOpen)
        {
            Debug.Log("[Door] ignorePlayerCollisionWhenOpen=FALSE, nu modific coliziunile", this);
            return;
        }

        if (playerColliders == null || doorColliders == null) return;

        Debug.Log($"[Door] Setez coliziuni: {(isOpen ? "IGNORE" : "ENABLE")}", this);

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