using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Death")]
    [SerializeField] private GameObject deathEffect; // Opțional
    [SerializeField] private float destroyDelay = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudio;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private Color originalColor;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (enemyRenderer == null)
            enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log($"[EnemyHealth] {gameObject.name} primește {damage} damage. HP: {currentHealth}/{maxHealth}");

        // Sunet hit
        if (hitAudio != null && hitSound != null)
            hitAudio.PlayOneShot(hitSound);

        // Flash roșu
        if (enemyRenderer != null)
        {
            StartCoroutine(HitFlash());
        }

        // Verifică moarte
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator HitFlash()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            enemyRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[EnemyHealth] {gameObject.name} MORT!");

        // Sunet death
        if (hitAudio != null && deathSound != null)
            hitAudio.PlayOneShot(deathSound);

        // Effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Dezactivează AI
        EnemyBrain brain = GetComponent<EnemyBrain>();
        if (brain != null) brain.enabled = false;

        // Dezactivează NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Oprește animații
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        // Distruge după delay
        Destroy(gameObject, destroyDelay);
    }

    // Pentru UI health bar (opțional)
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}