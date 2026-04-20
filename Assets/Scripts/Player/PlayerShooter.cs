using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Armas disponibles")]
    public WeaponData[] weapons;           // arrastra los 3 assets aquí
    public int currentWeaponIndex = 0;

    [Header("Referencias")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Object Pool")]
    public int poolSize = 40;

    private WeaponData currentWeapon;
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private GameObject[] bulletPool;
    private int poolIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        InitPool();
        EquipWeapon(currentWeaponIndex);
    }

    void InitPool()
    {
        bulletPool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            bulletPool[i] = Instantiate(bulletPrefab);
            bulletPool[i].SetActive(false);
        }
    }

    public void EquipWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0) return;
        currentWeaponIndex = Mathf.Clamp(index, 0, weapons.Length - 1);
        currentWeapon = weapons[currentWeaponIndex];
        Debug.Log($"Arma equipada: {currentWeapon.weaponName}");
    }

    void Update()
    {
        // Cambiar arma con rueda del ratón
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f) CycleWeapon(1);
        if (scroll < 0f) CycleWeapon(-1);

        // Disparar
        bool isShooting = Mouse.current.leftButton.isPressed;
        if (isShooting && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentWeapon.fireRate;
            Shoot();
        }
    }

    void CycleWeapon(int direction)
    {
        int newIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;
        EquipWeapon(newIndex);
    }

    void Shoot()
    {
        if (currentWeapon.isShootgun)
            ShootShotgun();
        else
            ShootSingle(GetMouseDirection());
    }

    void ShootSingle(Vector2 direction)
    {
        SpawnBullet(firePoint.position, direction);
    }

   void ShootShotgun()
{
    Vector2 baseDirection = GetMouseDirection();
    float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
    float halfSpread = currentWeapon.spreadAngle / 2f;

    for (int i = 0; i < currentWeapon.pelletCount; i++)
    {
        // Ángulo aleatorio dentro del rango de dispersión
        // en lugar de distribuir uniformemente
        float randomAngle = baseAngle + Random.Range(-halfSpread, halfSpread);

        // Pequeña variación extra de velocidad por bala
        // para que no lleguen todas al mismo tiempo
        float speedVariation = Random.Range(0.85f, 1.15f);

        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        SpawnBulletWithSpeed(firePoint.position, dir, speedVariation);
    }
}

    void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = GetPooledBullet();
        if (bullet == null) return;

        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(true);

        bullet.GetComponent<Bullet>().Launch(direction, currentWeapon);
    }

    Vector2 GetMouseDirection()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld  = mainCamera.ScreenToWorldPoint(mouseScreen);
        return ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;
    }

    GameObject GetPooledBullet()
    {
        for (int i = 0; i < poolSize; i++)
        {
            int index = (poolIndex + i) % poolSize;
            if (!bulletPool[index].activeInHierarchy)
            {
                poolIndex = (index + 1) % poolSize;
                return bulletPool[index];
            }
        }
        return null;
    }
    
    void SpawnBulletWithSpeed(Vector3 position, Vector2 direction, float speedMultiplier)
{
    GameObject bullet = GetPooledBullet();
    if (bullet == null) return;

    bullet.transform.position = position;
    bullet.transform.rotation = Quaternion.identity;
    bullet.SetActive(true);

    Bullet b = bullet.GetComponent<Bullet>();
    b.Launch(direction, currentWeapon);

    // Aplicar variación de velocidad después del Launch
    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
        rb.linearVelocity = direction * currentWeapon.bulletSpeed * speedMultiplier;
}
}