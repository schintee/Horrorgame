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

    [Header("Detect + Chase")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float loseSightRadius = 18f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Catch (Game Over)")]
    [SerializeField] private float catchDistance = 1.8f;
    [SerializeField] private float catchHoldTime = 0.9f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private int patrolIndex = -1;
    private bool chasing;
    private float catchTimer;
    private bool gameOverTriggered;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    IEnumerator Start()
    {
        // Find Player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent == null)
        {
            Debug.LogError("[EnemyBrain] No NavMeshAgent on Enemy.", this);
            yield break;
        }

        // Wait 1 frame (important sometimes when scene loads / navmesh initializes)
        yield return null;

        // Try to ensure agent is on NavMesh (multiple tries)
        bool onMesh = EnsureOnNavMesh(6f);
        if (!onMesh)
        {
            if (debugLogs) Debug.LogWarning("[EnemyBrain] Enemy is NOT on NavMesh. Will not patrol/chase until it is.", this);
            yield break;
        }

        agent.speed = patrolSpeed;

        // Start patrol if points exist
        if (patrolPoints != null && patrolPoints.Length > 0)
            GoNextPatrol();
        else if (debugLogs)
            Debug.LogWarning("[EnemyBrain] No patrol points set. Enemy will only chase if sees player.", this);
    }

    void Update()
    {
        if (gameOverTriggered) return;
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
        else if (chasing && !seesPlayer && dist > loseSightRadius)
        {
            chasing = false;
            agent.speed = patrolSpeed;
            catchTimer = 0f;
            if (debugLogs) Debug.Log("[EnemyBrain] STOP CHASE", this);

            if (patrolPoints != null && patrolPoints.Length > 0)
                GoNextPatrol();
        }

        if (chasing)
        {
            agent.SetDestination(player.position);

            if (dist <= catchDistance)
            {
                catchTimer += Time.deltaTime;
                if (catchTimer >= catchHoldTime)
                {
                    gameOverTriggered = true;
                    if (GameManager.Instance != null)
                        GameManager.Instance.GameOver();

                }
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

    private bool EnsureOnNavMesh(float maxDistance)
    {
        if (!agent.enabled) return false;
        if (agent.isOnNavMesh) return true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, maxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugLogs) Debug.Log($"[EnemyBrain] Warped to NavMesh at {hit.position}", this);
            return agent.isOnNavMesh;
        }

        return false;
    }

    private bool CanSeePlayer(float distanceToPlayer)
    {
        if (!chasing && distanceToPlayer > detectionRadius)
        {
            if (debugLogs) Debug.Log($"[EnemyBrain] No see: too far ({distanceToPlayer:F1} > {detectionRadius})", this);
            return false;
        }

        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 dir = (target - eye).normalized;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fieldOfView * 0.5f)
        {
            if (debugLogs) Debug.Log($"[EnemyBrain] No see: outside FOV (angle {angle:F0} > {fieldOfView * 0.5f:F0})", this);
            return false;
        }

        if (Physics.Raycast(eye, dir, distanceToPlayer, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log("[EnemyBrain] No see: obstacle blocking view (check Obstacle Layer!)", this);
            return false;
        }

        if (debugLogs) Debug.Log("[EnemyBrain] SEE PLAYER ✅", this);
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
