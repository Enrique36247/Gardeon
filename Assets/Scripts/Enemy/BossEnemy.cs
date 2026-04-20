using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossEnemy : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 60;

    [Header("Movimiento")]
    public float moveSpeed       = 1.2f;
    public float wanderInterval  = 2.5f;
    public float minPlayerDist   = 5f;

    [Header("Ataques")]
    public float attackInterval  = 1.5f;
    public GameObject bulletPrefab;

    [Header("Bala")]
    public float bulletSpeed    = 5f;
    public float bulletLifetime = 4f;
    public int   bulletDamage   = 1;
    public Color bulletColor    = new Color(1f, 0.1f, 0.8f);

    [Header("Fase 2")]
    public int   phase2Threshold = 30;   // vida a la que entra en fase 2
    public Color phase2Color     = new Color(1f, 0.3f, 0f);
    private bool isPhase2        = false;

    private Transform      player;
    private Rigidbody2D    rb;
    private Health         health;
    private SpriteRenderer sr;

    private Vector2 wanderDir     = Vector2.right;
    private float   wanderTimer   = 0f;
    private float   attackTimer   = 0f;
    private int     lastAttack    = -1;
    private bool    canAttack     = false;
    private bool    isDead        = false;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        sr     = GetComponent<SpriteRenderer>();

        health.maxHealth     = maxHealth;
        health.currentHealth = maxHealth;
        health.onDeath.AddListener(Die);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Esperar 2 segundos antes del primer ataque
        StartCoroutine(EnableAttackDelayed());
    }

    IEnumerator EnableAttackDelayed()
    {
        yield return new WaitForSeconds(2f);
        canAttack = true;
    }

    void Update()
    {
        if (isDead || player == null) return;

        CheckPhase2();
        HandleWander();
        HandleAttackTimer();
    }

    void CheckPhase2()
    {
        if (!isPhase2 && health.currentHealth <= phase2Threshold)
        {
            isPhase2 = true;
            moveSpeed      *= 1.5f;
            attackInterval *= 0.65f;
            if (sr != null) sr.color = phase2Color;
            StartCoroutine(Phase2Flash());
        }
    }

    IEnumerator Phase2Flash()
    {
        for (int i = 0; i < 6; i++)
        {
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (sr != null) sr.color = phase2Color;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void HandleWander()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval)
        {
            wanderTimer    = 0f;
            wanderInterval = Random.Range(1.5f, 3.5f);

            // Nueva dirección aleatoria con ligera orientación al jugador
            Vector2 toPlayer   = DirectionToPlayer();
            float   randomAngle = Random.Range(-110f, 110f);
            float   rad         = randomAngle * Mathf.Deg2Rad;
            wanderDir = new Vector2(
                toPlayer.x * Mathf.Cos(rad) - toPlayer.y * Mathf.Sin(rad),
                toPlayer.x * Mathf.Sin(rad) + toPlayer.y * Mathf.Cos(rad)
            ).normalized;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < minPlayerDist)
            rb.linearVelocity = -DirectionToPlayer() * moveSpeed;
        else
            rb.linearVelocity = wanderDir * moveSpeed;
    }

    void HandleAttackTimer()
    {
        if (!canAttack) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            ExecuteRandomAttack();
        }
    }

    void ExecuteRandomAttack()
    {
        // Elegir ataque al azar sin repetir el anterior
        List<int> options = new List<int> { 0, 1, 2, 3 };
        options.Remove(lastAttack);
        int chosen = options[Random.Range(0, options.Count)];
        lastAttack = chosen;

        switch (chosen)
        {
            case 0: StartCoroutine(AttackSpiral());     break;
            case 1: StartCoroutine(AttackBurst());      break;
            case 2: StartCoroutine(AttackAimed());      break;
            case 3: StartCoroutine(AttackCross());      break;
        }
    }

    // Ataque 0 — Espiral: dispara en círculo rotando el ángulo
    IEnumerator AttackSpiral()
    {
        int   totalShots  = isPhase2 ? 5 : 3;
        int   bulletsEach = 12;
        float angleOffset = 0f;

        for (int shot = 0; shot < totalShots; shot++)
        {
            float step = 360f / bulletsEach;
            for (int i = 0; i < bulletsEach; i++)
            {
                float angle = angleOffset + step * i;
                FireAt(AngleToDir(angle), speedMult: 1f);
            }
            angleOffset += 15f;
            yield return new WaitForSeconds(0.18f);
        }
    }

    // Ataque 1 — Burst: ráfaga densa hacia el jugador con dispersión
    IEnumerator AttackBurst()
    {
        int   waves       = isPhase2 ? 4 : 3;
        int   bulletsEach = isPhase2 ? 10 : 7;
        float spread      = 35f;

        for (int w = 0; w < waves; w++)
        {
            Vector2 baseDir  = DirectionToPlayer();
            float   baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            for (int i = 0; i < bulletsEach; i++)
            {
                float rnd = Random.Range(-spread, spread);
                FireAt(AngleToDir(baseAngle + rnd), speedMult: Random.Range(0.8f, 1.2f));
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Ataque 2 — Aimed: 3 balas muy rápidas directas al jugador
    IEnumerator AttackAimed()
    {
        int shots = isPhase2 ? 5 : 3;
        for (int i = 0; i < shots; i++)
        {
            FireAt(DirectionToPlayer(), speedMult: 2.2f);
            yield return new WaitForSeconds(0.12f);
        }
    }

    // Ataque 3 — Cruz: 8 balas en cruz + diagonal, luego rota 45 grados
    IEnumerator AttackCross()
    {
        int   waves      = isPhase2 ? 3 : 2;
        float baseAngle  = Random.Range(0f, 45f);

        for (int w = 0; w < waves; w++)
        {
            int directions = isPhase2 ? 12 : 8;
            float step     = 360f / directions;

            for (int i = 0; i < directions; i++)
                FireAt(AngleToDir(baseAngle + step * i), speedMult: 1f);

            baseAngle += 22.5f;
            yield return new WaitForSeconds(0.25f);
        }
    }

    void FireAt(Vector2 direction, float speedMult = 1f)
    {
        if (bulletPrefab == null || isDead) return;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            transform.position + (Vector3)(direction * 0.8f),
            Quaternion.identity
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed    = bulletSpeed * speedMult;
            bullet.lifetime = bulletLifetime;
            bullet.damage   = bulletDamage;
            bullet.piercing = false;

            SpriteRenderer bsr = bulletObj.GetComponent<SpriteRenderer>();
            if (bsr != null) bsr.color = isPhase2
                ? new Color(1f, 0.4f, 0f)
                : bulletColor;

            bullet.Launch(direction);

            Rigidbody2D brb = bulletObj.GetComponent<Rigidbody2D>();
            if (brb != null)
                brb.linearVelocity = direction * bullet.speed;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Flash de muerte
        for (int i = 0; i < 8; i++)
        {
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (sr != null) sr.color = Color.black;
            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.3f);

        if (GameManager.Instance != null)
            GameManager.Instance.Victory();

        Destroy(gameObject);
    }

    Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.right;
        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    Vector2 AngleToDir(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}