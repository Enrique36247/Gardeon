using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;
    public Button          restartButton;
    public Button          menuButton;

    void Start()
    {
        // Recuperar stats de la run
        int rooms  = PlayerPrefs.GetInt("RoomsCleared", 0);
        int kills  = PlayerPrefs.GetInt("TotalKills",   0);

        if (statsText != null)
            statsText.text = $"Salas completadas: {rooms}\nEnemigos eliminados: {kills}";

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);
    }

    void Restart()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("SampleScene");
    }

    void GoToMenu()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("MainMenu");
    }
}