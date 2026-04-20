using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Dash / Roll")]
    public float dashSpeed      = 14f;
    public float dashDuration   = 0.18f;
    public float dashCooldown   = 0.7f;

    private bool    isDashing     = false;
    private float   dashTimer     = 0f;
    private float   cooldownTimer = 0f;
    private Vector2 dashDirection;
    private Color   originalColor;

    private Rigidbody2D    rb;
    private Vector2        moveInput;
    private Camera         mainCamera;
    private Health         health;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        mainCamera     = Camera.main;
        health         = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor  = spriteRenderer.color;
    }

    void OnEnable()
    {
        if (health != null)
            health.onDamaged.AddListener(OnPlayerDamaged);
    }

    void OnDisable()
    {
        if (health != null)
            health.onDamaged.RemoveListener(OnPlayerDamaged);
    }

    void Update()
    {
        Vector2 keyboardInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x =  1f;

            float y = 0f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  y = -1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    y =  1f;

            keyboardInput = new Vector2(x, y);
        }
        moveInput = keyboardInput.normalized;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        bool dashPressed = Keyboard.current != null &&
                           (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                            Keyboard.current.spaceKey.wasPressedThisFrame);

        if (dashPressed && !isDashing && cooldownTimer <= 0f)
            StartDash();

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        if (!isDashing)
            RotateTowardsMouse();
    }

    void FixedUpdate()
    {
        if (isDashing)
            rb.linearVelocity = dashDirection * dashSpeed;
        else
            rb.linearVelocity = moveInput * moveSpeed;

        ClampPosition();
    }

    void ClampPosition()
    {
        if (Camera.main == null) return;

        float camH     = Camera.main.orthographicSize;
        float camW     = camH * Camera.main.aspect;
        Vector3 camPos = Camera.main.transform.position;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, camPos.x - camW + 0.5f, camPos.x + camW - 0.5f);
        pos.y = Mathf.Clamp(pos.y, camPos.y - camH + 0.5f, camPos.y + camH - 0.5f);
        transform.position = pos;
    }

    void StartDash()
    {
        isDashing     = true;
        dashTimer     = dashDuration;
        cooldownTimer = dashCooldown;

        dashDirection = moveInput.magnitude > 0.1f
            ? moveInput
            : (Vector2)transform.up;

        if (health != null)
            health.SetInvulnerable(true);

        // Ignorar colisiones con balas enemigas durante el dash
        int playerLayer      = LayerMask.NameToLayer("Player");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        if (playerLayer >= 0 && enemyBulletLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyBulletLayer, true);

        StopAllCoroutines();
        StartCoroutine(DashVisual());
    }

    void EndDash()
    {
        isDashing = false;

        if (health != null)
            health.SetInvulnerable(false);

        // Restaurar colisiones con balas enemigas al terminar el dash
        int playerLayer      = LayerMask.NameToLayer("Player");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        if (playerLayer >= 0 && enemyBulletLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyBulletLayer, false);
    }

    void OnPlayerDamaged(int currentHealth)
    {
        StopAllCoroutines();
        StartCoroutine(DamageSequence());
    }

    IEnumerator DamageSequence()
    {
        ClearEnemyBullets();

        float elapsed    = 0f;
        float iFramesDur = health != null ? health.iFramesDuration : 0.5f;

        while (elapsed < iFramesDur)
        {
            elapsed += Time.deltaTime;

            bool showRed = Mathf.FloorToInt(elapsed / 0.08f) % 2 == 0;
            if (spriteRenderer != null)
                spriteRenderer.color = showRed
                    ? new Color(1f, 0.2f, 0.2f)
                    : new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);

            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void ClearEnemyBullets()
    {
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        if (enemyBulletLayer < 0) return;

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.layer == enemyBulletLayer && obj.activeInHierarchy)
                obj.SetActive(false);
        }
    }

    IEnumerator DashVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0.35f
            );

        yield return new WaitForSeconds(dashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void RotateTowardsMouse()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld  = mainCamera.ScreenToWorldPoint(mouseScreen);
        Vector2 direction   = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}