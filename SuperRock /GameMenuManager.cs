using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameMenuManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    public Button newGameButton;
    public Button continueButton;
    public Button exitButton;
    public TextMeshProUGUI titleText;
    public GameObject mainMenuPanel;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button saveButton;
    public Button menuButton;
    public Button resumeButton;
    public Button quitButton;

    [Header("In-Game UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI healthText;
    public GameObject deathScreen;
    public TextMeshProUGUI diedText;

    private bool isPaused = false;

    [Header("DestroyObject")]
    public GameObject objectToDestroy;

    void Start()
    {
        // Èíèöèàëèçàöèÿ ìåíþ
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            InitializeMainMenu();
        }
        else
        {
            InitializeGameUI();
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            UpdateUI();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    void InitializeMainMenu()
    {
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
        }

        bool hasSave = SaveManager.Instance.HasSave();
        continueButton.interactable = hasSave;

        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        exitButton.onClick.AddListener(OnExit);

        mainMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    void InitializeGameUI()
    {
        if (deathScreen != null)
            deathScreen.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Íàñòðîéêà êíîïîê ïàóçû
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + PlayerPrefs.GetInt("Money", 0);
        }

        if(objectToDestroy != null)
        {
            if(PlayerPrefs.GetInt("Money", 0) > 9)
            {
               Destroy(objectToDestroy);
            }
        }

        if (healthText != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterControl character = player.GetComponent<CharacterControl>();
                if (character != null)
                {
                    healthText.text = "HP: " + character.GetCurrentHealth();
                }
            }
        }
    }

    public void UpdateHealthUI(int health)
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + health;
        }
    }


    // Îñíîâíûå ìåòîäû ìåíþ
    public void OnNewGame()
    {
        SaveManager.Instance.DeleteAllSaves();
        PlayerPrefs.SetInt("Money", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1");
    }

    public void OnContinue()
    {
        string levelToLoad = SaveManager.Instance.GetSavedLevel();
        SceneManager.LoadScene(levelToLoad);
    }

    public void OnExit()
    {
        Debug.Log("Âûõîäèì èç èãðû");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Ìåòîäû ïàóçû
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        if (saveButton != null)
        {
            TextMeshProUGUI text = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "ÑÎÕÐÀÍÅÍÎ!";
                Invoke("ResetSaveButtonText", 1f);
            }
        }
    }

    void ResetSaveButtonText()
    {
        if (saveButton != null)
        {
            TextMeshProUGUI text = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = "Ñîõðàíèòü";
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SaveGame();
        SceneManager.LoadScene("Menu");
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void QuitGame()
    {
        SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        CharacterControl.canWalk = !isPaused;
    }

    // Ìåòîäû ýêðàíà ñìåðòè
    public void ShowDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            StartCoroutine(DeathScreenAnimation());
        }
    }

    private System.Collections.IEnumerator DeathScreenAnimation()
    {
        CanvasGroup canvasGroup = deathScreen.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = deathScreen.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Çàòåìíåíèå
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 0.8f, elapsed / duration);
            yield return null;
        }

        // Òåêñò
        if (diedText != null)
        {
            diedText.text = "ÂÛ ÓÌÅÐËÈ";
            diedText.color = new Color(1, 0, 0, 0);

            elapsed = 0f;
            duration = 1f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                diedText.color = new Color(1, 0, 0, elapsed / duration);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(2f);

        // Ïåðåçàãðóçêà
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
