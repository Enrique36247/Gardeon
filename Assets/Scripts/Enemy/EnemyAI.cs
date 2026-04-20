using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 3f;     // distancia a la que deja de acercarse

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;
    public float fireRange = 6f;        // rango máximo para disparar

    [Header("Referencias")]
    public Transform player;

    private float nextFireTime = 0f;
    private Rigidbody2D rb;
    private Health health;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        // Suscribirse al evento de muerte
        health.onDeath.AddListener(Die);
    }

    void Start()
    {
        // Buscar al jugador automáticamente si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        RotateTowardsPlayer();
        HandleMovement(distanceToPlayer);
        HandleShooting(distanceToPlayer);
    }

    void RotateTowardsPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    void HandleMovement(float distance)
    {
        if (distance > stopDistance)
        {
            // Perseguir al jugador
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Quedarse quieto cuando está cerca
            rb.linearVelocity = Vector2.zero;
        }
    }

    void HandleShooting(float distance)
    {
        if (distance > fireRange) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        ShootAtPlayer();
    }

   void ShootAtPlayer()
{
    if (bulletPrefab == null) return;

    Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    GameObject bulletObj = Instantiate(
        bulletPrefab,
        transform.position + (Vector3)(direction * 0.6f),
        Quaternion.Euler(0f, 0f, angle - 90f)
    );

    Bullet bullet = bulletObj.GetComponent<Bullet>();
    if (bullet != null)
    {
        // Configurar manualmente sin WeaponData
        bullet.speed    = 6f;
        bullet.lifetime = 3f;
        bullet.damage   = 1;
        bullet.piercing = false;

        bullet.Launch(direction);
    }
}

    void Die()
    {
        // Aquí más adelante añadiremos efectos de partículas y drops
        Destroy(gameObject);
    }
}