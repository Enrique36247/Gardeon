using UnityEngine;

public class EnemySpinner : EnemyBase
{
    [Header("Spinner")]
    public int   bulletsPerBurst  = 8;
    public float spreadRandomness = 25f;  // variación aleatoria por bala en grados
    public int   burstCount       = 3;
    public float pauseDuration    = 2.5f;
    public float rotationSpeed    = 45f;  // más lento que antes

    private float currentAngle = 0f;
    private int   burstsLeft;
    private bool  isPausing    = false;
    private float pauseTimer   = 0f;

    // Movimiento lento y errático
    private Vector2 wanderDirection = Vector2.right;
    private float   wanderTimer     = 0f;
    private float   wanderInterval  = 2f;

    protected override void Awake()
    {
        base.Awake();
        bulletColor  = new Color(0.8f, 0f, 0.8f);
        bulletSpeed  = 4f;     // balas lentas
        bulletDamage = 1;
        fireRate     = 0.5f;
        moveSpeed    = 1.2f;   // el más lento de todos
        burstsLeft   = burstCount;
    }

    void Update()
    {
        if (player == null) return;

        HandleWander();
        HandleShooting();

        // Rotar el ángulo base del patrón
        currentAngle += rotationSpeed * Time.deltaTime;
    }

    void HandleWander()
    {
        // Movimiento lento y errático — cambia dirección cada wanderInterval segundos
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval)
        {
            wanderTimer = 0f;
            wanderInterval = Random.Range(1.5f, 3f);

            // Nueva dirección aleatoria ligeramente orientada al jugador
            Vector2 toPlayer = DirectionToPlayer();
            float randomAngle = Random.Range(-90f, 90f);
            float rad = randomAngle * Mathf.Deg2Rad;
            wanderDirection = new Vector2(
                toPlayer.x * Mathf.Cos(rad) - toPlayer.y * Mathf.Sin(rad),
                toPlayer.x * Mathf.Sin(rad) + toPlayer.y * Mathf.Cos(rad)
            ).normalized;
        }

        // Mantener cierta distancia mínima del jugador
        float dist = DistanceToPlayer();
        if (dist < 3f)
            rb.linearVelocity = -DirectionToPlayer() * moveSpeed;
        else
            rb.linearVelocity = wanderDirection * moveSpeed;
    }

    void HandleShooting()
    {
        if (isPausing)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPausing  = false;
                burstsLeft = burstCount;
            }
            return;
        }

        if (!canShoot) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            FireBurst();

            burstsLeft--;
            if (burstsLeft <= 0)
            {
                isPausing  = true;
                pauseTimer = pauseDuration;
            }
        }
    }

    void FireBurst()
    {
        float angleStep = 360f / bulletsPerBurst;

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            // Ángulo base uniforme + variación aleatoria = patrón caótico
            float baseAngle   = currentAngle + angleStep * i;
            float randomAngle = baseAngle + Random.Range(-spreadRandomness, spreadRandomness);

            // Variación de velocidad por bala
            float speedVariation = Random.Range(0.8f, 1.3f);

            float rad = randomAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            FireBulletWithSpeed(dir, speedVariation);
        }
    }

    void FireBulletWithSpeed(Vector2 direction, float speedMultiplier)
    {
        if (bulletPrefab == null) return;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            transform.position + (Vector3)(direction * 0.6f),
            Quaternion.identity
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed    = bulletSpeed * speedMultiplier;
            bullet.lifetime = bulletLifetime;
            bullet.damage   = bulletDamage;
            bullet.piercing = false;

            SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = bulletColor;

            bullet.Launch(direction);

            // Aplicar velocidad con variación después del Launch
            Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = direction * bulletSpeed * speedMultiplier;
        }
    }
}