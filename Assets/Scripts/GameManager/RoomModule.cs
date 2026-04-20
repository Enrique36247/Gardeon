using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomModule : MonoBehaviour
{
    [Header("Dimensiones")]
    public float width         = 28f;
    public float height        = 28f;
    public float wallThickness = 0.8f;
    public float doorWidth     = 3f;

    [Header("Puertas activas")]
    public bool doorNorth = false;
    public bool doorSouth = false;
    public bool doorEast  = false;
    public bool doorWest  = false;

    [Header("Colores")]
    public Color wallColor  = new Color(0.25f, 0.25f, 0.3f);
    public Color floorColor = new Color(0.1f,  0.1f,  0.13f);
    public Color doorColor  = new Color(0.1f,  0.6f,  0.2f);

    [Header("Spawn de enemigos")]
    public List<GameObject> enemyPrefabs       = new List<GameObject>();
    public int   enemyCount                    = 3;
    public float spawnMargin                   = 3f;
    public float minDistanceFromPlayer         = 6f;
    public float spawnDelay                    = 2f;
    public float shootDelay                    = 1f;

    [Header("Siguiente sala")]
    public RoomModule nextRoom;
    public float exitSpawnOffset = 3f;

    public bool IsCleared { get; private set; } = false;

    private bool isActive                    = false;
    private bool spawnSequenceRunning        = false;
    private List<DoorModule> doors           = new List<DoorModule>();
    private List<GameObject> spawnedEnemies  = new List<GameObject>();
    private List<GameObject> spawnIndicators = new List<GameObject>();

    // Paredes sólidas que bloquean la puerta mientras hay enemigos
    private List<GameObject> doorBlockers    = new List<GameObject>();

    public void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        doors.Clear();
        spawnedEnemies.Clear();
        spawnIndicators.Clear();
        doorBlockers.Clear();

        BuildRoom();
    }

    void BuildRoom()
    {
        CreateFloor();

        CreateWallOrDoor("North",
            centerX: 0f, centerY: height / 2f - wallThickness / 2f,
            totalLength: width, thickness: wallThickness,
            horizontal: true, hasDoor: doorNorth, isPositive: true);

        CreateWallOrDoor("South",
            centerX: 0f, centerY: -(height / 2f - wallThickness / 2f),
            totalLength: width, thickness: wallThickness,
            horizontal: true, hasDoor: doorSouth, isPositive: false);

        CreateWallOrDoor("East",
            centerX: width / 2f - wallThickness / 2f, centerY: 0f,
            totalLength: height, thickness: wallThickness,
            horizontal: false, hasDoor: doorEast, isPositive: true);

        CreateWallOrDoor("West",
            centerX: -(width / 2f - wallThickness / 2f), centerY: 0f,
            totalLength: height, thickness: wallThickness,
            horizontal: false, hasDoor: doorWest, isPositive: false);
    }

    void CreateFloor()
    {
        CreateSegment("Floor", 0f, 0f, width, height,
                      floorColor, hasCollider: false,
                      sortOrder: -1, isTrigger: false);
    }

    void CreateWallOrDoor(string name, float centerX, float centerY,
                           float totalLength, float thickness,
                           bool horizontal, bool hasDoor, bool isPositive)
    {
        if (!hasDoor)
        {
            float w = horizontal ? totalLength : thickness;
            float h = horizontal ? thickness   : totalLength;
            CreateSegment(name, centerX, centerY, w, h,
                          wallColor, hasCollider: true,
                          sortOrder: 0, isTrigger: false);
            return;
        }

        float segLength = (totalLength - doorWidth) / 2f;
        float segOffset = segLength / 2f + doorWidth / 2f;

        if (horizontal)
        {
            CreateSegment(name + "_L", centerX - segOffset, centerY,
                          segLength, thickness,
                          wallColor, hasCollider: true, sortOrder: 0, isTrigger: false);

            CreateSegment(name + "_R", centerX + segOffset, centerY,
                          segLength, thickness,
                          wallColor, hasCollider: true, sortOrder: 0, isTrigger: false);
        }
        else
        {
            CreateSegment(name + "_B", centerX, centerY - segOffset,
                          thickness, segLength,
                          wallColor, hasCollider: true, sortOrder: 0, isTrigger: false);

            CreateSegment(name + "_T", centerX, centerY + segOffset,
                          thickness, segLength,
                          wallColor, hasCollider: true, sortOrder: 0, isTrigger: false);
        }

        // Bloqueador sólido — bloquea físicamente la puerta mientras hay enemigos
        float bw = horizontal ? doorWidth      : thickness;
        float bh = horizontal ? thickness      : doorWidth;
        GameObject blocker = CreateSegment(name + "_Blocker",
            centerX, centerY, bw, bh,
            wallColor, hasCollider: true, sortOrder: 0, isTrigger: false);
        doorBlockers.Add(blocker);

        // Puerta trigger — solo activa cuando sala limpia
        float dw = horizontal ? doorWidth     : thickness * 2f;
        float dh = horizontal ? thickness * 2f : doorWidth;
        GameObject doorObj = CreateSegment(name + "_Door",
            centerX, centerY, dw, dh,
            doorColor, hasCollider: true, sortOrder: 1, isTrigger: true);

        // Puerta empieza invisible y desactivada
        doorObj.GetComponent<SpriteRenderer>().enabled = false;
        doorObj.GetComponent<BoxCollider2D>().enabled  = false;

        DoorModule door      = doorObj.AddComponent<DoorModule>();
        door.parentRoom      = this;
        door.isHorizontal    = horizontal;
        door.isPositiveDir   = isPositive;
        door.exitSpawnOffset = exitSpawnOffset;
        doors.Add(door);
    }

    GameObject CreateSegment(string segName, float localX, float localY,
                              float w, float h, Color color,
                              bool hasCollider, int sortOrder, bool isTrigger)
    {
        GameObject obj = new GameObject(segName);
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(localX, localY, 0f);
        obj.transform.localScale    = Vector3.one;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite       = GetWhiteSprite();
        sr.color        = color;
        sr.sortingOrder = sortOrder;
        sr.drawMode     = SpriteDrawMode.Sliced;
        sr.size         = new Vector2(w, h);

        if (hasCollider)
        {
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size          = new Vector2(w, h);
            col.isTrigger     = isTrigger;
        }

        return obj;
    }

    Sprite GetWhiteSprite()
    {
        Texture2D tex  = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4),
                             new Vector2(0.5f, 0.5f), 4f, 0,
                             SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));
    }

    // --- Estado ---

    public void ActivateRoom()
    {
        isActive  = true;
        IsCleared = false;
        spawnSequenceRunning = true;
        gameObject.SetActive(true);

        // Puertas bloqueadas físicamente e invisibles
        SetDoorState(open: false);

        StartCoroutine(SpawnSequence());
    }

    public void DeactivateRoom()
    {
        isActive = false;
        spawnSequenceRunning = false;
        StopAllCoroutines();

        foreach (var e in spawnedEnemies)
            if (e != null) Destroy(e);
        foreach (var ind in spawnIndicators)
            if (ind != null) Destroy(ind);

        spawnedEnemies.Clear();
        spawnIndicators.Clear();
        gameObject.SetActive(false);
    }

    // Abre o cierra las puertas visualmente y físicamente
    void SetDoorState(bool open)
    {
        foreach (var door in doors)
        {
            SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
            BoxCollider2D  col = door.GetComponent<BoxCollider2D>();
            if (sr  != null) sr.enabled  = open;
            if (col != null) col.enabled = open;
        }

        // Bloqueador: sólido cuando cerrado, desactivado cuando abierto
        foreach (var blocker in doorBlockers)
        {
            if (blocker != null)
                blocker.GetComponent<BoxCollider2D>().enabled = !open;
        }
    }

    IEnumerator SpawnSequence()
    {
        if (enemyPrefabs.Count == 0)
        {
            spawnSequenceRunning = false;
            ClearRoom();
            yield break;
        }

        List<Vector3> spawnPositions = GenerateSpawnPositions();

        // Mostrar indicadores
        foreach (var pos in spawnPositions)
            spawnIndicators.Add(CreateSpawnIndicator(pos));

        // Animar indicadores
        float elapsed = 0f;
        while (elapsed < spawnDelay)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / spawnDelay;

            foreach (var ind in spawnIndicators)
            {
                if (ind == null) continue;
                SpriteRenderer sr = ind.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                float blinkSpeed = Mathf.Lerp(2f, 14f, t);
                float alpha      = Mathf.Abs(Mathf.Sin(elapsed * blinkSpeed));
                sr.color = new Color(1f, 0.2f, 0.2f, alpha * 0.7f);

                float scale = Mathf.Lerp(0.4f, 0.9f, t);
                ind.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }

        foreach (var ind in spawnIndicators)
            if (ind != null) Destroy(ind);
        spawnIndicators.Clear();

        spawnedEnemies.Clear();
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy  = Instantiate(prefab, spawnPositions[i],
                                            Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
            StartCoroutine(EnableShootingDelayed(enemy));
        }

        spawnSequenceRunning = false;
    }

    List<Vector3> GenerateSpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        float halfW       = width  / 2f - spawnMargin - wallThickness;
        float halfH       = height / 2f - spawnMargin - wallThickness;
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player")?.transform.position
                            ?? transform.position;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 candidate = Vector3.zero;
            bool found        = false;

            for (int attempt = 0; attempt < 40; attempt++)
            {
                candidate = transform.position + new Vector3(
                    Random.Range(-halfW, halfW),
                    Random.Range(-halfH, halfH),
                    0f
                );

                if (Vector3.Distance(candidate, playerPos) < minDistanceFromPlayer)
                    continue;

                bool tooClose = false;
                foreach (var existing in positions)
                {
                    if (Vector3.Distance(candidate, existing) < 2.5f)
                    { tooClose = true; break; }
                }

                if (!tooClose) { found = true; break; }
            }

            if (!found)
                candidate = transform.position + new Vector3(
                    Random.Range(-halfW, halfW),
                    Random.Range(-halfH, halfH), 0f);

            positions.Add(candidate);
        }

        return positions;
    }

    GameObject CreateSpawnIndicator(Vector3 worldPos)
    {
        GameObject ind = new GameObject("SpawnIndicator");
        ind.transform.SetParent(transform, false);
        ind.transform.position   = worldPos;
        ind.transform.localScale = Vector3.one * 0.5f;

        SpriteRenderer sr = ind.AddComponent<SpriteRenderer>();
        sr.sprite       = GetWhiteSprite();
        sr.color        = new Color(1f, 0.2f, 0.2f, 0.5f);
        sr.sortingOrder = 2;
        sr.drawMode     = SpriteDrawMode.Sliced;
        sr.size         = Vector2.one;

        return ind;
    }

    IEnumerator EnableShootingDelayed(GameObject enemy)
    {
        EnemyBase eb = enemy?.GetComponent<EnemyBase>();
        if (eb != null) eb.canShoot = false;

        yield return new WaitForSeconds(shootDelay);

        if (enemy != null && eb != null)
            eb.canShoot = true;
    }

    void Update()
    {
        if (!isActive || IsCleared || spawnSequenceRunning) return;
        spawnedEnemies.RemoveAll(e => e == null);
        if (spawnedEnemies.Count == 0)
            ClearRoom();
    }

    void ClearRoom()
    {
        IsCleared = true;
        SetDoorState(open: true);
    }
}
