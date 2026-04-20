using UnityEngine;

public class EnemyGrunt : EnemyBase
{
    [Header("Grunt")]
    public float stopDistance  = 4f;   // distancia a la que se detiene
    public float retreatDistance = 2.5f; // si el jugador se acerca más, retrocede

    protected override void Awake()
    {
        base.Awake();
        bulletColor = Color.red;
        fireRate    = 1.2f;
        moveSpeed   = 2.5f;
        fireRange   = 6f;
    }

    void Update()
    {
        if (player == null) return;

        RotateTowardsPlayer();
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float dist = DistanceToPlayer();

        if (dist < retreatDistance)
        {
            // Demasiado cerca — retroceder
            rb.linearVelocity = -DirectionToPlayer() * moveSpeed;
        }
        else if (dist < stopDistance)
        {
            // En rango óptimo — quedarse quieto y disparar
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Lejos — acercarse
            rb.linearVelocity = DirectionToPlayer() * moveSpeed;
        }
    }

    void HandleShooting()
    {
        if (DistanceToPlayer() > fireRange) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        FireBullet(DirectionToPlayer());
    }
}