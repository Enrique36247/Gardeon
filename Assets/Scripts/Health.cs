using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 6;
    public int currentHealth;

    [Header("Armadura")]
    public int armor = 0;

    [Header("Invulnerabilidad")]
    public bool useDamageIFrames = true;
    public float iFramesDuration = 0.5f;
    private bool isInvulnerable = false;
    private float iFramesTimer = 0f;

    [Header("Eventos")]
    public UnityEvent onDeath;
    public UnityEvent<int> onDamaged;
    public UnityEvent<int> onArmorDamaged;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (iFramesTimer > 0f)
        {
            iFramesTimer -= Time.deltaTime;
            if (iFramesTimer <= 0f)
                isInvulnerable = false;
        }
    }

    // Used by dash to manually control invulnerability.
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
        iFramesTimer = 0f;
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        if (armor > 0)
        {
            int absorbed = Mathf.Min(armor, amount);
            armor -= absorbed;
            amount -= absorbed;
            onArmorDamaged?.Invoke(armor);
        }

        if (amount <= 0) return;

        currentHealth -= amount;
        onDamaged?.Invoke(currentHealth);

        if (useDamageIFrames && iFramesDuration > 0f)
        {
            isInvulnerable = true;
            iFramesTimer = iFramesDuration;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            onDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void AddArmor(int amount)
    {
        armor += amount;
    }
}
