using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float chaseRadius = 15f;
    [SerializeField] private float loseSightRadius = 20f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Transform player;
    private LanternSystem playerLantern;

    private int currentPatrolIndex = -1;
    private bool isChasing;

    private void Awake()
    {
        // Auto-assign agent if inspector is empty
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        FindPlayer();

        if (agent == null)
        {
            Debug.LogError("[EnemyAI] No NavMeshAgent found on enemy!", this);
            enabled = false;
            return;
        }

        // Snap to NavMesh if needed
        EnsureOnNavMesh();

        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("[EnemyAI] No patrol points assigned. Enemy will not move.", this);
            return;
        }

        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        bool canSee = CanSeePlayer(dist);

        if (canSee)
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
                if (debugLogs) Debug.Log("[EnemyAI] START CHASE", this);
            }
        }
        else if (isChasing && dist > loseSightRadius)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            if (debugLogs) Debug.Log("[EnemyAI] STOP CHASE", this);
            GoToNextPatrolPoint();
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.6f)
                GoToNextPatrolPoint();
        }
    }

    private void EnsureOnNavMesh()
    {
        if (agent.isOnNavMesh) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugLogs) Debug.Log($"[EnemyAI] Warped to NavMesh at {hit.position}", this);
        }
        else
        {
            Debug.LogWarning("[EnemyAI] Enemy is NOT on NavMesh and couldn't find nearby NavMesh. Bake navmesh / move enemy onto it.", this);
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            if (debugLogs) Debug.LogWarning("[EnemyAI] No object with tag 'Player' found.", this);
            return;
        }

        player = p.transform;
        playerLantern = p.GetComponentInChildren<LanternSystem>();
    }

    private bool CanSeePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > chaseRadius) return false;

        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 dir = (target - eye).normalized;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fieldOfView * 0.5f) return false;

        // Obstacle check
        if (Physics.Raycast(eye, dir, distanceToPlayer, obstacleLayer, QueryTriggerInteraction.Ignore))
            return false;

        float lightBonus = 0f;
        if (playerLantern != null && playerLantern.IsOn)
        {
            // BatteryPercent la tine e 0..1
            lightBonus = 3f * playerLantern.BatteryPercent;
        }

        float effectiveDetection = detectionRadius + lightBonus;
        return distanceToPlayer <= effectiveDetection;
    }

    private void GoToNextPatrolPoint()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (debugLogs) Debug.Log($"[EnemyAI] Patrol -> {patrolPoints[currentPatrolIndex].name}", this);
    }
}
