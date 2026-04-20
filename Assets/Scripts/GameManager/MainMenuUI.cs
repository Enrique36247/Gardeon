using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI titleText;
    public Button          playButton;
    public Button          quitButton;

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        if (Keyboard.current != null &&
           (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    void QuitGame()
    {
        Application.Quit();
    }
}