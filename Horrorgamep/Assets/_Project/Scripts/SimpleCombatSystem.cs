using UnityEngine;

public class SimpleCombatSystem : MonoBehaviour
{
    [Header("Atac Settings")]
    [SerializeField] private float attackRange = 5f; // Mai mare!
    [SerializeField] private float attackRadius = 2f; // Arie larga
    [SerializeField] private float attackDamage = 34f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private KeyCode attackKey = KeyCode.F; // Tasta F

    [Header("Audio")]
    [SerializeField] private AudioSource attackAudio;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private float lastAttackTime;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (showDebug)
        {
            Debug.Log("[Combat] Combat System ready! Press F to attack");
        }
    }

    void Update()
    {
        // UI pentru a vedea ce e in fata ta
        CheckForEnemy();

        // Atac
        if (Input.GetKeyDown(attackKey))
        {
            if (Time.time - lastAttackTime < attackCooldown)
            {
                if (showDebug) Debug.Log("[Combat] Asteapta cooldown!");
                return;
            }

            PerformAttack();
        }
    }

    private void CheckForEnemy()
    {
        // Cautare cu SphereCast (mai larg)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, attackRadius, out hit, attackRange))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy == null)
                enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null && UIManager.Instance != null)
            {
                UIManager.Instance.ShowEnemyHealth(enemy);
                return;
            }
        }

        // Nu e nimic
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideEnemyHealth();
        }
    }

    private void PerformAttack()
    {
        if (showDebug) Debug.Log("[Combat] ========== ATAC ==========");

        lastAttackTime = Time.time;

        // Sunet
        if (attackAudio != null && attackSound != null)
            attackAudio.PlayOneShot(attackSound);

        // Cautare cu SphereCast LARG
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, attackRadius, out hit, attackRange))
        {
            if (showDebug) Debug.Log($"[Combat] SphereCast lovit: {hit.collider.name}");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy == null)
                enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                if (showDebug) Debug.Log($"[Combat] ✓✓✓ DAMAGE LA ENEMY: {attackDamage}");

                enemy.TakeDamage(attackDamage);

                if (attackAudio != null && hitSound != null)
                    attackAudio.PlayOneShot(hitSound);

                return;
            }
        }

        // Daca SphereCast nu merge, incearca sa gasesti orice enemy aproape
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider col in nearbyColliders)
        {
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();
            if (enemy == null)
                enemy = col.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                // Verifica daca e in fata
                Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                Vector3 forward = transform.forward;

                float angle = Vector3.Angle(forward, dirToEnemy);

                if (angle < 90f) // In fata
                {
                    if (showDebug) Debug.Log($"[Combat] ✓✓✓ BACKUP HIT pe {enemy.name}");

                    enemy.TakeDamage(attackDamage);

                    if (attackAudio != null && hitSound != null)
                        attackAudio.PlayOneShot(hitSound);

                    return;
                }
            }
        }

        if (showDebug) Debug.Log("[Combat] ✗ Miss - niciun enemy aproape");
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        // Arata aria de atac
        Gizmos.color = Color.red;
        Vector3 direction = playerCamera.transform.forward;
        Gizmos.DrawRay(playerCamera.transform.position, direction * attackRange);

        // Sfera de cautare
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(playerCamera.transform.position + direction * attackRange, attackRadius);
    }
}