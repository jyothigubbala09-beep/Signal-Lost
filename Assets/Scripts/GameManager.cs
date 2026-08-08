using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool isGameOver = false;
    private float elapsedTime = 0f;
    private bool isTimerActive = false;
    private int movesCount = 0;

    [Header("Star Limits")]
    public float threeStarTimeLimit = 90f;
    public int threeStarMovesLimit = 35;
    public float twoStarTimeLimit = 180f;
    public int twoStarMovesLimit = 70;

    [Header("HUD UI")]
    public Text statusText; // "TIME: 00:00    |    MOVES: 0    |    TOWERS: 0/3"
    public Text signalStrengthText; // "SIGNAL STATUS: 0% [ALERT: RESTORE GRID]"

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public Text victoryTitleText; // "LEVEL COMPLETE!"
    public Text victoryStarsText; // "★ ★ ★"
    public Text victoryDescText; // "Time: 02:18\nMoves: 10"
    public Button nextLevelButton;
    public Button restartButton; // "RETRY"
    public Button mainMenuButton; // "HOME"

    private NetworkManager networkManager;

    void Start()
    {
        isGameOver = false;
        elapsedTime = 0f;
        movesCount = 0;
        isTimerActive = true;

        networkManager = FindObjectOfType<NetworkManager>();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(NextLevel);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        UpdateHUD();
    }

    void Update()
    {
        if (isTimerActive && !isGameOver)
        {
            elapsedTime += Time.deltaTime;
            UpdateHUD();
        }
    }

    public void IncrementMoves()
    {
        if (!isGameOver)
        {
            movesCount++;
            UpdateHUD();
        }
    }

    public void OnTowerRestored(Tower tower)
    {
        UpdateHUD();

        if (networkManager != null)
        {
            int active = networkManager.GetActiveDamagedTowersCount();
            int total = networkManager.GetTotalDamagedTowersCount();

            if (active >= total && total > 0)
            {
                TriggerVictory();
            }
        }
    }

    public void UpdateHUD()
    {
        int active = 0;
        int total = 0;
        float signal = 0f;

        if (networkManager != null)
        {
            active = networkManager.GetActiveDamagedTowersCount();
            total = networkManager.GetTotalDamagedTowersCount();
            signal = networkManager.GetSignalStrength();
        }

        if (statusText != null)
        {
            statusText.text = $"TIME: {FormatTime(elapsedTime)}    |    MOVES: {movesCount}    |    TOWERS: {active}/{total}";
        }

        if (signalStrengthText != null)
        {
            string alertStatus = signal >= 100f ? "SECURE" : "ALERT: RESTORE GRID";
            signalStrengthText.text = $"SIGNAL STATUS: {signal:0}% [{alertStatus}]";
        }
    }

    void TriggerVictory()
    {
        isGameOver = true;
        isTimerActive = false;

        // Disable player movement
        var player = FindObjectOfType<DroneController>();
        if (player != null)
        {
            player.enabled = false;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        // Play level complete victory chime
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioEvent.LevelComplete);
        }

        // Calculate Stars
        int stars = 1;
        if (elapsedTime <= threeStarTimeLimit && movesCount <= threeStarMovesLimit)
        {
            stars = 3;
        }
        else if (elapsedTime <= twoStarTimeLimit && movesCount <= twoStarMovesLimit)
        {
            stars = 2;
        }

        // Set visual elements on Victory Panel
        if (victoryTitleText != null)
        {
            victoryTitleText.text = "LEVEL COMPLETE!";
        }

        if (victoryStarsText != null)
        {
            switch (stars)
            {
                case 3:
                    victoryStarsText.text = "★ ★ ★";
                    victoryStarsText.color = new Color(1.0f, 0.85f, 0.1f); // Golden Yellow
                    break;
                case 2:
                    victoryStarsText.text = "★ ★ ☆";
                    victoryStarsText.color = new Color(0.9f, 0.75f, 0.15f);
                    break;
                default:
                    victoryStarsText.text = "★ ☆ ☆";
                    victoryStarsText.color = new Color(0.7f, 0.7f, 0.7f); // Muted Silver/Grey
                    break;
            }
        }

        if (victoryDescText != null)
        {
            victoryDescText.text = $"Time: {FormatTime(elapsedTime)}\nMoves: {movesCount}";
        }

        // Save progression data
        int levelNum = LevelManager.currentLevel;
        int previousBest = PlayerPrefs.GetInt($"Level_{levelNum}_Stars", 0);
        if (stars > previousBest)
        {
            PlayerPrefs.SetInt($"Level_{levelNum}_Stars", stars);
        }

        if (levelNum < 5)
        {
            PlayerPrefs.SetInt($"Level_{levelNum + 1}_Unlocked", 1);
        }
        PlayerPrefs.Save();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void NextLevel()
    {
        if (LevelManager.currentLevel < 5)
        {
            LevelManager.currentLevel++;
            SceneManager.LoadScene("Game");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
