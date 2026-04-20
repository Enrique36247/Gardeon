using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Salas")]
    public RoomModule[] rooms;
    private int currentRoomIndex = 0;
    private RoomModule currentRoom;

    [Header("Referencias")]
    public GameObject player;
    public GameObject fadePanel;

    [Header("Stats de run")]
    public int roomsCleared = 0;
    public int totalKills   = 0;

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        foreach (var room in rooms)
        {
            room.gameObject.SetActive(true);
            room.Build();
            room.gameObject.SetActive(false);
        }

        ActivateRoom(0);
    }

    void ActivateRoom(int index)
    {
        currentRoomIndex = index;
        currentRoom      = rooms[index];
        currentRoom.ActivateRoom();

        Camera.main.transform.position = new Vector3(
            currentRoom.transform.position.x,
            currentRoom.transform.position.y,
            -10f
        );
    }

    public void TransitionToRoom(RoomModule nextRoom, Vector3 spawnPosition)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine(nextRoom, spawnPosition));
    }

    IEnumerator TransitionCoroutine(RoomModule nextRoom, Vector3 spawnPosition)
    {
        isTransitioning = true;

        yield return StartCoroutine(Fade(0f, 1f, 0.3f));

        currentRoom.DeactivateRoom();
        player.transform.position = spawnPosition;

        int nextIndex = System.Array.IndexOf(rooms, nextRoom);
        ActivateRoom(nextIndex);
        roomsCleared++;

        yield return StartCoroutine(Fade(1f, 0f, 0.3f));

        isTransitioning = false;
    }

    public void RegisterKill()
    {
        totalKills++;
    }

    public void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    public void Victory()
    {
        StartCoroutine(VictoryCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        // Guardar stats
        PlayerPrefs.SetInt("RoomsCleared", roomsCleared);
        PlayerPrefs.SetInt("TotalKills",   totalKills);
        PlayerPrefs.Save();

        // Fade lento a negro y esperar un momento para que se sienta dramático
        yield return StartCoroutine(Fade(0f, 1f, 1.5f));
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("GameOver");
    }

    IEnumerator VictoryCoroutine()
    {
        PlayerPrefs.SetInt("RoomsCleared", roomsCleared);
        PlayerPrefs.SetInt("TotalKills",   totalKills);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(Fade(0f, 1f, 1.5f));
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadePanel == null) yield break;
        CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        // Asegurarse de que el panel es visible durante el fade
        fadePanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed  += Time.deltaTime;
            cg.alpha  = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}