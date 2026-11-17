using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera deathCamera;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (playerCamera != null)
        {
            playerCamera.enabled = true;

            AudioListener playerListener =
                playerCamera.GetComponent<AudioListener>();

            if (playerListener != null)
            {
                playerListener.enabled = true;
            }
        }

        if (deathCamera != null)
        {
            deathCamera.enabled = false;

            AudioListener deathListener =
                deathCamera.GetComponent<AudioListener>();

            if (deathListener != null)
            {
                deathListener.enabled = false;
            }
        }

        Debug.Log($"Health initialized: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(
            $"Player took {damage} damage. " +
            $"Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log(
            $"Player healed by {amount}. " +
            $"Health: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Debug.Log("Player died.");

        if (playerCamera != null)
        {
            playerCamera.enabled = false;

            AudioListener playerListener =
                playerCamera.GetComponent<AudioListener>();

            if (playerListener != null)
            {
                playerListener.enabled = false;
            }
        }

        if (deathCamera != null)
        {
            deathCamera.gameObject.SetActive(true);
            deathCamera.enabled = true;

            AudioListener deathListener =
                deathCamera.GetComponent<AudioListener>();

            if (deathListener != null)
            {
                deathListener.enabled = true;
            }

            Debug.Log("Death camera activated.");
        }
        else
        {
            Debug.LogError(
                "Death Camera is not assigned on PlayerHealth.");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Destroy(gameObject);
    }
}