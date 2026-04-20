using UnityEngine;

public class DoorModule : MonoBehaviour
{
    [HideInInspector] public RoomModule parentRoom;
    [HideInInspector] public bool       isHorizontal;
    [HideInInspector] public bool       isPositiveDir;
    [HideInInspector] public float      exitSpawnOffset = 3f;

    private BoxCollider2D col;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    public void SetLocked(bool locked)
    {
        // Estado gestionado por RoomModule.
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (parentRoom == null || parentRoom.nextRoom == null)
        {
            Debug.LogWarning("DoorModule: falta parentRoom o nextRoom");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("DoorModule: GameManager no encontrado");
            return;
        }

        // Seguridad extra: nunca avanzar si la sala no esta limpia.
        if (!parentRoom.IsCleared)
            return;

        Vector3 spawnPos = GetSpawnPosition();
        GameManager.Instance.TransitionToRoom(parentRoom.nextRoom, spawnPos);
    }

    Vector3 GetSpawnPosition()
    {
        RoomModule next   = parentRoom.nextRoom;
        Vector3    center = next.transform.position;
        Vector3    spawn  = center;

        if (isHorizontal)
        {
            float y = isPositiveDir
                ? center.y - (next.height / 2f - next.wallThickness - exitSpawnOffset)
                : center.y + (next.height / 2f - next.wallThickness - exitSpawnOffset);
            spawn = new Vector3(center.x, y, 0f);
        }
        else
        {
            float x = isPositiveDir
                ? center.x - (next.width / 2f - next.wallThickness - exitSpawnOffset)
                : center.x + (next.width / 2f - next.wallThickness - exitSpawnOffset);
            spawn = new Vector3(x, center.y, 0f);
        }

        return ClampInsideRoom(next, spawn);
    }

    Vector3 ClampInsideRoom(RoomModule room, Vector3 worldPos)
    {
        const float padding = 0.6f;

        Vector3 center = room.transform.position;
        float halfW = Mathf.Max(0f, room.width / 2f - room.wallThickness - padding);
        float halfH = Mathf.Max(0f, room.height / 2f - room.wallThickness - padding);

        float x = Mathf.Clamp(worldPos.x, center.x - halfW, center.x + halfW);
        float y = Mathf.Clamp(worldPos.y, center.y - halfH, center.y + halfH);

        return new Vector3(x, y, 0f);
    }
}
