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
    [SerializeField] private float patrolSpeed = 1.6f;

    [Header("Chase")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float chaseSpeed = 2.6f;   // MAI LENT
    [SerializeField] private float fieldOfView = 140f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Catch (RESPAWN)")]
    [SerializeField] private float catchDistance = 1.6f; // MAI SIGUR
    [SerializeField] private float catchHoldTime = 0.8f;

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject respawnCanvas;
    [SerializeField] private float respawnCanvasTime = 1.2f;
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

        // IMPORTANT: sincronizam agentul cu catch-ul
        agent.stoppingDistance = 0.8f;
        agent.autoBraking = true;
    }

    IEnumerator Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        yield return null;

        if (!EnsureOnNavMesh(6f)) yield break;

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
        if (player == null) return;

        // DISTANTA PE PLAN (fara Y)
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0; b.y = 0;
        float dist = Vector3.Distance(a, b);

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

        // DOAR OBSTACOLE (NU PLAYER)
        if (Physics.Raycast(eye, dir, distanceToPlayer, obstacleLayer, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

    private IEnumerator RespawnPlayerRoutine()
    {
        if (respawning) yield break;
        respawning = true;

        chasing = false;
        catchTimer = 0f;

        if (respawnCanvas != null)
            respawnCanvas.SetActive(true);

        yield return new WaitForSeconds(respawnCanvasTime);

        if (respawnPoint != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = respawnPoint.position;
            player.rotation = respawnPoint.rotation;

            if (cc != null) cc.enabled = true;
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

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return agent.isOnNavMesh;
        }
        return false;
    }

    private void GoNextPatrol()
    {
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }
}
