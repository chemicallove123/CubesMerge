using UnityEngine;

public class EnemyType : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(
            currentHealth,
            0f
        );

        Debug.Log(
            $"[EnemyType] {gameObject.name} took " +
            $"{damage} damage. " +
            $"Health: {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(
            $"[EnemyType] {gameObject.name} died."
        );

        Destroy(gameObject);
    }
}