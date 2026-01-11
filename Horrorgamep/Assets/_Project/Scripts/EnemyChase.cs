using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Catch Settings")]
    [Tooltip("Cat de aproape trebuie sa fie enemy-ul ca sa inceapa sa te prinda")]
    [SerializeField] private float catchDistance = 1.8f;

    [Tooltip("Cat timp trebuie sa stea aproape ca sa te omoare")]
    [SerializeField] private float catchHoldTime = 0.8f;

    private float catchTimer = 0f;
    private bool gameOverTriggered = false;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (agent != null)
        {
            agent.speed = chaseSpeed;

            // siguranta: daca nu e pe NavMesh, incearca sa-l puna
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }
    }

    void Update()
    {
        if (gameOverTriggered) return;
        if (agent == null || player == null) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Urmareste player-ul
        agent.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= catchDistance)
        {
            catchTimer += Time.deltaTime;

            // OPTIONAL: debug
            // Debug.Log($"Catching... {catchTimer:F2}");

            if (catchTimer >= catchHoldTime)
            {
                gameOverTriggered = true;
                GameManager.Instance?.GameOver();
            }
        }
        else
        {
            // daca te indepartezi, reseteaza timerul
            catchTimer = 0f;
        }
    }
}
