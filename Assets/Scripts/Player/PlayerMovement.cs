using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Dash / Roll")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.7f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector2 dashDirection;
    private Color originalColor;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;
    private Health health;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
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

    // Límite de seguridad — clamp dentro de la cámara
    ClampPosition();
}

void ClampPosition()
{
    if (Camera.main == null) return;

    float camH = Camera.main.orthographicSize;
    float camW = camH * Camera.main.aspect;
    Vector3 camPos = Camera.main.transform.position;

    Vector3 pos = transform.position;
    pos.x = Mathf.Clamp(pos.x, camPos.x - camW + 0.5f, camPos.x + camW - 0.5f);
    pos.y = Mathf.Clamp(pos.y, camPos.y - camH + 0.5f, camPos.y + camH - 0.5f);
    transform.position = pos;
}

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        dashDirection = moveInput.magnitude > 0.1f
            ? moveInput
            : (Vector2)transform.up;

        if (health != null)
            health.SetInvulnerable(true);

        StopAllCoroutines();
        StartCoroutine(DashVisual());
    }

    void EndDash()
    {
        isDashing = false;

        if (health != null)
            health.SetInvulnerable(false);
    }

    System.Collections.IEnumerator DashVisual()
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
