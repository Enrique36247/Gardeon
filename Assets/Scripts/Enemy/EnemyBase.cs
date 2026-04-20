using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed  = 2.5f;
    public float fireRate   = 1.5f;
    public float fireRange  = 8f;
    public bool  canShoot   = true;   // controlado por RoomModule al spawnear
    public GameObject bulletPrefab;

    [Header("Bala enemiga")]
    public float bulletSpeed    = 6f;
    public float bulletLifetime = 3f;
    public int   bulletDamage   = 1;
    public Color bulletColor    = Color.red;

    protected Transform player;
    protected Rigidbody2D rb;
    protected Health health;
    protected float nextFireTime = 0f;

    protected virtual void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (health != null)
        {
            // Enemigos sin iFrames para que cada impacto cuente.
            health.useDamageIFrames = false;
            health.onDeath.AddListener(Die);
        }
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    protected void RotateTowardsPlayer()
    {
        Vector2 dir   = DirectionToPlayer();
        float angle   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected void FireBullet(Vector2 direction, float angleOffset = 0f)
    {
        if (!canShoot) return;
        if (bulletPrefab == null) return;

        if (angleOffset != 0f)
        {
            float rad = angleOffset * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            direction = new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos
            );
        }

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            transform.position + (Vector3)(direction * 0.6f),
            Quaternion.identity
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed    = bulletSpeed;
            bullet.lifetime = bulletLifetime;
            bullet.damage   = bulletDamage;
            bullet.piercing = false;

            SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = bulletColor;

            bullet.Launch(direction);
        }
    }

    protected virtual void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        Destroy(gameObject);
    }
}
