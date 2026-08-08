using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Panel")]
    public GameObject mainMenuPanel;
    public Button startButton;   // "PLAY"
    public Button levelsButton;  // "LEVELS"
    public Button settingsButton;// "SETTINGS"
    public Button quitButton;    // "EXIT"

    [Header("Level Select Panel")]
    public GameObject levelSelectPanel;
    public Button[] levelButtons; // Array of 5 level buttons
    public Text[] starTexts; // Array of 5 star text fields
    public Button backButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button resetProgressButton;
    public Button settingsBackButton;

    void Start()
    {
        // Unlock Level 1 by default
        if (PlayerPrefs.GetInt("Level_1_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("Level_1_Unlocked", 1);
            PlayerPrefs.Save();
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(PlayGame);
        }

        if (levelsButton != null)
        {
            levelsButton.onClick.AddListener(OpenLevelSelect);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseLevelSelect);
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(CloseSettings);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.AddListener(ResetProgress);
        }

        // Initialize panel states
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        SetupLevelButtons();
    }

    void SetupLevelButtons()
    {
        if (levelButtons == null || starTexts == null) return;

        for (int i = 0; i < 5; i++)
        {
            if (i >= levelButtons.Length || i >= starTexts.Length) break;

            int levelNum = i + 1;
            bool isUnlocked = PlayerPrefs.GetInt($"Level_{levelNum}_Unlocked", 0) == 1 || levelNum == 1;

            if (isUnlocked)
            {
                levelButtons[i].interactable = true;
                int stars = PlayerPrefs.GetInt($"Level_{levelNum}_Stars", 0);
                
                // Format star display (e.g. ★ ★ ☆)
                string starStr = "";
                for (int s = 0; s < 3; s++)
                {
                    starStr += (s < stars) ? "★ " : "☆ ";
                }
                starTexts[i].text = starStr.Trim();
                starTexts[i].color = new Color(1.0f, 0.85f, 0.1f); // Golden Yellow
            }
            else
            {
                levelButtons[i].interactable = false;
                starTexts[i].text = "LOCKED";
                starTexts[i].color = new Color(0.5f, 0.5f, 0.5f); // Grey
            }

            // Bind click handler
            levelButtons[i].onClick.RemoveAllListeners();
            int capturedLvl = levelNum;
            levelButtons[i].onClick.AddListener(() => LoadLevel(capturedLvl));
        }
    }

    public void PlayGame()
    {
        // Clicking Play Game automatically loads the highest unlocked level
        int highestUnlocked = 1;
        for (int i = 1; i <= 5; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{i}_Unlocked", 0) == 1)
            {
                highestUnlocked = i;
            }
        }
        LoadLevel(highestUnlocked);
    }

    public void OpenLevelSelect()
    {
        SetupLevelButtons();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("Level_1_Unlocked", 1);
        PlayerPrefs.Save();
        SetupLevelButtons(); // Refresh buttons locks representation
        CloseSettings();
    }

    void LoadLevel(int levelNum)
    {
        LevelManager.currentLevel = levelNum;
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
