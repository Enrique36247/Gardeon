using UnityEngine;

public class EnemySniper : EnemyBase
{
    [Header("Sniper")]
    public float retreatDistance   = 6f;    // huye si el jugador se acerca
    public float preferredDistance = 10f;   // distancia ideal para disparar

    [Header("Telegrafía")]
    public float telegraphTime = 1f;
    private bool  isTelegraphing  = false;
    private float telegraphTimer  = 0f;
    private Color originalColor;

    protected override void Awake()
    {
        base.Awake();
        bulletColor  = new Color(1f, 0.5f, 0f);
        bulletSpeed  = 16f;
        bulletDamage = 2;
        fireRate     = 3f;
        moveSpeed    = 2.8f;
        fireRange    = 14f;
    }

    protected override void Start()
    {
        base.Start();
        originalColor = GetComponent<SpriteRenderer>()?.color ?? Color.white;
    }

    void Update()
    {
        if (player == null) return;

        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float dist = DistanceToPlayer();

        if (dist < retreatDistance)
        {
            // Huir activamente
            rb.linearVelocity = -DirectionToPlayer() * moveSpeed * 1.3f;
        }
        else if (dist > preferredDistance)
        {
            // Acercarse hasta distancia preferida
            rb.linearVelocity = DirectionToPlayer() * moveSpeed;
        }
        else
        {
            // En distancia ideal — moverse lateralmente para ser difícil de acertar
            Vector2 lateral = new Vector2(-DirectionToPlayer().y, DirectionToPlayer().x);
            rb.linearVelocity = lateral * moveSpeed * 0.6f;
            RotateTowardsPlayer();
        }
    }

    void HandleShooting()
    {
        if (DistanceToPlayer() > fireRange) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (isTelegraphing)
        {
            telegraphTimer -= Time.deltaTime;

            if (sr != null)
                sr.color = Mathf.Sin(telegraphTimer * 20f) > 0
                    ? new Color(1f, 0.5f, 0f)
                    : Color.white;

            if (telegraphTimer <= 0f)
            {
                isTelegraphing = false;
                if (sr != null) sr.color = originalColor;
                FireBullet(DirectionToPlayer());
                nextFireTime = Time.time + fireRate;
            }
        }
        else if (Time.time >= nextFireTime)
        {
            isTelegraphing = true;
            telegraphTimer = telegraphTime;
        }
    }
}