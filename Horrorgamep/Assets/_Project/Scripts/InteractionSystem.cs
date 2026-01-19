using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Keys")]
    [SerializeField] private KeyCode doorKey = KeyCode.E;     // usi
    [SerializeField] private KeyCode pickupKey = KeyCode.P;   // pickup (chei + obiecte)

    [Header("Door Interaction")]
    [SerializeField] private float doorRange = 4.5f;
    [SerializeField] private float doorSphereRadius = 1.0f;
    [SerializeField] private LayerMask doorMask = ~0;

    [Header("Pickup Interaction")]
    [SerializeField] private float pickupRange = 3.0f;
    [SerializeField] private float pickupSphereRadius = 1.5f;
    [SerializeField] private LayerMask pickupMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private int playerLayer;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>(true);
        }

        playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            doorMask &= ~(1 << playerLayer);
            pickupMask &= ~(1 << playerLayer);
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;

        if (Input.GetKeyDown(pickupKey))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            TryPickup(ray);
            return;
        }

        if (Input.GetKeyDown(doorKey))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            TryDoor(ray);
        }
    }

    private void TryPickup(Ray ray)
    {
        // cautam intr-o "bula" mare in fata camerei
        Vector3 probeCenter = ray.origin + ray.direction * Mathf.Min(pickupRange, 2.0f);

        Collider[] hits = Physics.OverlapSphere(
            probeCenter,
            pickupSphereRadius,
            pickupMask,
            QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0)
        {
            if (debugLogs) Debug.Log("[InteractionSystem] No pickup colliders found.");
            return;
        }

        // alegem cel mai apropiat pickup valid
        float bestDist = float.MaxValue;
        KeyPickup bestKey = null;
        CollectibleItem bestCollectible = null;

        foreach (var c in hits)
        {
            if (c == null) continue;

            var key = c.GetComponentInParent<KeyPickup>();
            if (key != null && key.isActiveAndEnabled && key.gameObject.activeInHierarchy)
            {
                float d = Vector3.Distance(transform.position, key.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestKey = key;
                    bestCollectible = null;
                }
                continue;
            }

            var col = c.GetComponentInParent<CollectibleItem>();
            if (col != null && col.isActiveAndEnabled && col.gameObject.activeInHierarchy)
            {
                float d = Vector3.Distance(transform.position, col.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestCollectible = col;
                    bestKey = null;
                }
            }
        }

        // ridicare: incercam metodele existente in proiectul tau
        if (bestKey != null)
        {
            if (debugLogs) Debug.Log($"[InteractionSystem] Pick KEY: {bestKey.name}");
            // daca ai versiunea mea: PickUpFromInteract()
            bestKey.SendMessage("PickUpFromInteract", SendMessageOptions.DontRequireReceiver);
            // daca ai versiunea veche (trigger pickup), nu se intampla nimic aici -> schimba KeyPickup.cs cum ti-am dat
            return;
        }

        if (bestCollectible != null)
        {
            if (debugLogs) Debug.Log($"[InteractionSystem] Pick COLLECTIBLE: {bestCollectible.name}");
            // versiunea mea: PickUpFromInteract()
            bestCollectible.SendMessage("PickUpFromInteract", SendMessageOptions.DontRequireReceiver);
            return;
        }

        if (debugLogs) Debug.Log("[InteractionSystem] Nothing pickable nearby.");
    }

    private void TryDoor(Ray ray)
    {
        if (Physics.SphereCast(ray, doorSphereRadius, out RaycastHit hit, doorRange, doorMask, QueryTriggerInteraction.Ignore))
        {


            Door door = hit.collider.GetComponentInParent<Door>();

            if (door != null)
            {
                if (debugLogs) Debug.Log($"[InteractionSystem] Door interact: {door.name}");
                door.Interact();
            }
        }
        else
        {
            if (debugLogs) Debug.Log("[InteractionSystem] No door hit.");
        }
    }
}
