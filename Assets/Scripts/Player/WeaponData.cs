using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Identidad")]
    public string weaponName = "Pistola";
    public Color bulletColor = Color.yellow;

    [Header("Disparo")]
    public float fireRate       = 0.15f;
    public float bulletSpeed    = 15f;
    public float bulletLifetime = 2f;
    public int   damage         = 1;
    public bool  piercingBullet = false;  // bala que atraviesa (rifle)

    [Header("Escopeta")]
    public bool  isShootgun     = false;
    public int   pelletCount    = 6;      // balas por disparo
    public float spreadAngle    = 30f;    // ángulo total del abanico
}