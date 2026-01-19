using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2.2f;

    [Header("Chase")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float chaseSpeed = 4.5f;
    [SerializeField] private float fieldOfView = 140f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Catch (RESPAWN)")]
    [SerializeField] private float catchDistance = 2f;
    [SerializeField] private float catchHoldTime = 1f;

    [Tooltip("Unde respawneaza playerul cand e prins")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Canvas-ul care apare cand e respawn (SetActive(false) by default)")]
    [SerializeField] private GameObject respawnCanvas;

    [Tooltip("Cat timp sta pe ecran canvas-ul de respawn")]
    [SerializeField] private float respawnCanvasTime = 1.5f;

    [Tooltip("Cooldown ca sa nu te prinda imediat dupa respawn")]
    [SerializeField] private float catchCooldown = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private int patrolIndex = -1;
    private bool chasing;
    private float catchTimer;
    private bool respawning;
    private float cooldownTimer;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    IEnumerator Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent == null)
        {
            Debug.LogError("[EnemyBrain] No NavMeshAgent", this);
            yield break;
        }

        yield return null;

        bool onMesh = EnsureOnNavMesh(6f);
        if (!onMesh)
        {
            if (debugLogs) Debug.LogWarning("[EnemyBrain] Not on NavMesh", this);
            yield break;
        }

        agent.speed = patrolSpeed;

        if (patrolPoints != null && patrolPoints.Length > 0)
            GoNextPatrol();

        if (respawnCanvas != null)
            respawnCanvas.SetActive(false);
    }

    void Update()
    {
        if (respawning) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        bool seesPlayer = CanSeePlayer(dist);

        if (!chasing && seesPlayer)
        {
            chasing = true;
            agent.speed = chaseSpeed;
            if (debugLogs) Debug.Log("[EnemyBrain] START CHASE", this);
        }

        if (chasing)
        {
            agent.SetDestination(player.position);

            if (dist <= catchDistance)
            {
                catchTimer += Time.deltaTime;
                if (catchTimer >= catchHoldTime)
                    StartCoroutine(RespawnPlayerRoutine());
            }
            else
            {
                catchTimer = 0f;
            }
        }
        else
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                if (!agent.pathPending && agent.remainingDistance < 0.6f)
                    GoNextPatrol();
            }
        }
    }

    private IEnumerator RespawnPlayerRoutine()
    {
        if (respawning) yield break;
        respawning = true;

        if (debugLogs) Debug.Log("[EnemyBrain] CAUGHT -> FULL RESET + RESPAWN", this);

        chasing = false;
        catchTimer = 0f;

        // 1) RESET KEY INVENTORY (doors)
        var inv = player.GetComponentInParent<PlayerInventory>();
        if (inv != null) inv.ResetInventory();

        // 2) RESET UI INVENTORY (items shown in inventory panel)
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.ClearInventory();

        // 3) RESET collected counter in GameManager (optional but recommended)
        if (GameManager.Instance != null)
            GameManager.Instance.ResetCollectedObjects();

        // 4) RESPAWN ALL COLLECTIBLES (bring them back in scene)
        foreach (var c in FindObjectsOfType<CollectibleItem>(true))
            c.ResetItem();

        // 5) RESPAWN ALL KEYS
        foreach (var k in FindObjectsOfType<KeyPickup>(true))
            k.ResetKey();

        // UI feedback
        if (respawnCanvas != null)
            respawnCanvas.SetActive(true);

        yield return new WaitForSeconds(respawnCanvasTime);

        // teleport player
        if (respawnPoint != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = respawnPoint.position;
            player.rotation = respawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("[EnemyBrain] RespawnPoint is NULL - set it in Inspector!", this);
        }

        if (respawnCanvas != null)
            respawnCanvas.SetActive(false);

        cooldownTimer = catchCooldown;

        agent.speed = patrolSpeed;
        if (patrolPoints != null && patrolPoints.Length > 0)
            GoNextPatrol();

        respawning = false;
    }

    private bool EnsureOnNavMesh(float maxDistance)
    {
        if (!agent.enabled) return false;
        if (agent.isOnNavMesh) return true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, maxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugLogs) Debug.Log($"[EnemyBrain] Warped to {hit.position}", this);
            return agent.isOnNavMesh;
        }

        return false;
    }

    private bool CanSeePlayer(float distanceToPlayer)
    {
        if (!chasing && distanceToPlayer > detectionRadius)
            return false;

        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 dir = (target - eye).normalized;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fieldOfView * 0.5f)
            return false;

        if (Physics.Raycast(eye, dir, distanceToPlayer, obstacleLayer, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

    private void GoNextPatrol()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);

        if (debugLogs) Debug.Log($"[EnemyBrain] Patrol -> {patrolPoints[patrolIndex].name}", this);
    }
}
