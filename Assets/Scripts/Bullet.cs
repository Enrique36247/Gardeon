using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float speed = 15f;
    [HideInInspector] public float lifetime = 2f;
    [HideInInspector] public int damage = 1;
    [HideInInspector] public bool piercing = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool firedByPlayer = true;

    private int playerLayer;
    private int enemyLayer;
    private int playerBulletLayer;
    private int enemyBulletLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        playerBulletLayer = LayerMask.NameToLayer("PlayerBullet");
        enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");

        // Fallback by prefab layer in case Launch has not run yet.
        firedByPlayer = gameObject.layer != enemyBulletLayer;
    }

    void OnDisable()
    {
        CancelInvoke();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // Player bullets.
    public void Launch(Vector2 direction, WeaponData data)
    {
        firedByPlayer = true;

        speed = data.bulletSpeed;
        lifetime = data.bulletLifetime;
        damage = data.damage;
        piercing = data.piercingBullet;

        if (playerBulletLayer >= 0)
            gameObject.layer = playerBulletLayer;

        if (sr != null)
            sr.color = data.bulletColor;

        rb.linearVelocity = direction * speed;
        ScheduleDeactivate();
    }

    // Enemy bullets.
    public void Launch(Vector2 direction)
    {
        firedByPlayer = false;

        if (enemyBulletLayer >= 0)
            gameObject.layer = enemyBulletLayer;

        rb.linearVelocity = direction * speed;
        ScheduleDeactivate();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldIgnoreCollision(other))
            return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);

            if (!piercing)
                Deactivate();
        }
        else
        {
            Deactivate();
        }
    }

    bool ShouldIgnoreCollision(Collider2D other)
    {
        if (other == null) return true;

        int otherLayer = other.gameObject.layer;

        // Avoid bullet-vs-bullet despawns.
        if (otherLayer == playerBulletLayer || otherLayer == enemyBulletLayer)
            return true;

        if (firedByPlayer)
        {
            // Player bullets should not hit the player.
            if (otherLayer == playerLayer)
                return true;
        }
        else
        {
            // Enemy bullets should not hit enemies.
            if (otherLayer == enemyLayer)
                return true;
        }

        return false;
    }

    void ScheduleDeactivate()
    {
        CancelInvoke();
        if (lifetime > 0f)
            Invoke(nameof(Deactivate), lifetime);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
