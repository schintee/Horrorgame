using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();

    [Header("Collectibles")]
    [SerializeField] private int collectedCount = 0;
    public int CollectedCount => collectedCount;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showKeysInInspector = true;

    // Pentru a vedea cheile în Inspector
    [SerializeField] private List<string> currentKeys = new List<string>();

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerInventory] Inițializat pe '{gameObject.name}'!", this);
        }
    }

    private void Update()
    {
        // Update Inspector list pentru debug
        if (showKeysInInspector)
        {
            currentKeys.Clear();
            currentKeys.AddRange(keys);
        }
    }

    private string Normalize(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? "" : id.Trim().ToLowerInvariant();
    }

    public bool HasKey(string keyID)
    {
        keyID = Normalize(keyID);
        bool has = !string.IsNullOrEmpty(keyID) && keys.Contains(keyID);

        if (debugMode)
        {
            Debug.Log($"[PlayerInventory] HasKey('{keyID}') = {has}. Chei actuale: [{string.Join(", ", keys)}]", this);
        }

        return has;
    }

    public void AddKey(string keyID)
    {
        string original = keyID;
        keyID = Normalize(keyID);

        if (string.IsNullOrEmpty(keyID))
        {
            Debug.LogWarning($"[PlayerInventory] Încercare de adăugare cheie invalidă: '{original}'", this);
            return;
        }

        bool wasNew = keys.Add(keyID);

        if (debugMode)
        {
            if (wasNew)
                Debug.Log($"[PlayerInventory] ✓ Cheia '{keyID}' ADĂUGATĂ! Chei totale: {keys.Count} [{string.Join(", ", keys)}]", this);
            else
                Debug.Log($"[PlayerInventory] Cheia '{keyID}' deja există în inventar.", this);
        }
    }

    public bool TryConsumeKey(string keyID)
    {
        keyID = Normalize(keyID);
        if (string.IsNullOrEmpty(keyID)) return false;

        bool removed = keys.Remove(keyID);

        if (debugMode && removed)
        {
            Debug.Log($"[PlayerInventory] Cheia '{keyID}' consumată/ștearsă!", this);
        }

        return removed;
    }

    public void RemoveKey(string keyID)
    {
        keyID = Normalize(keyID);
        if (string.IsNullOrEmpty(keyID)) return;

        bool removed = keys.Remove(keyID);

        if (debugMode && removed)
        {
            Debug.Log($"[PlayerInventory] Cheia '{keyID}' ștearsă!", this);
        }
    }

    public void AddCollectible(int amount = 1)
    {
        collectedCount += Mathf.Max(1, amount);

        if (debugMode)
        {
            Debug.Log($"[PlayerInventory] Collectible adăugat. Total: {collectedCount}", this);
        }
    }

    public void ResetInventory()
    {
        keys.Clear();
        collectedCount = 0;

        if (debugMode)
        {
            Debug.Log("[PlayerInventory] Inventar resetat complet!", this);
        }
    }

    // Metodă de debug pentru a afișa toate cheile
    [ContextMenu("Show All Keys")]
    public void ShowAllKeys()
    {
        Debug.Log($"[PlayerInventory] Chei în inventar ({keys.Count}): [{string.Join(", ", keys)}]", this);
    }
}