using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TextMeshProUGUI wonLevel;

    [Header("Settings UI")]
    [SerializeField] private Button sfxToggle;
    [SerializeField] private Button musicToggle;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;

    private ColorChanger[] bottles;
    private Image musicImage;
    private Image sfxImage;
    private bool isSettingsOpen;
    private bool isPaused;

    public int Level;
    public static int PersistentLevel;
    public static GameManager instance;

    // Events for settings changes
    public System.Action<bool> OnMusicToggled;
    public System.Action<bool> OnSfxToggled;

    private void Awake()
    {
        instance = this;
        InitializeAudioUI();
    }

    void Start()
    {
        Time.timeScale = 1f;
        Level = PersistentLevel > 0 ? PersistentLevel : LevelSelection.Instance.currentLevel;

        InitializeSettings();
        SetupAudioListeners();
        StartCoroutine(WaitForBottleSpawn());

        // Start background music for the level
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBackgroundMusic(0); // You can vary this by level
        }
    }

    private void InitializeAudioUI()
    {
        // Cache Image components for toggle buttons
        if (musicToggle != null)
            musicImage = musicToggle.GetComponent<Image>();
        if (sfxToggle != null)
            sfxImage = sfxToggle.GetComponent<Image>();
    }

    private void InitializeSettings()
    {
        // Load settings and update UI
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        bool isSfxOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;

        UpdateMusicSprite(isMusicOn);
        UpdateSfxSprite(isSfxOn);

        // Ensure panels start closed
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void SetupAudioListeners()
    {
        // Setup button listeners
        if (sfxToggle != null)
            sfxToggle.onClick.AddListener(ToggleSfx);
        if (musicToggle != null)
            musicToggle.onClick.AddListener(ToggleMusic);
    }

    IEnumerator WaitForBottleSpawn()
    {
        yield return new WaitForSeconds(1.5f);
        bottles = FindObjectsByType<ColorChanger>(FindObjectsSortMode.None);
    }

    public void CheckGameOver()
    {
        if (AllBottlesInValidState())
        {
            Debug.Log("Level Complete!");

            // Play completion sound
            if (MusicManager.Instance != null && levelCompleteSound != null)
            {
                MusicManager.Instance.PlaySFX(levelCompleteSound, 0.8f);
            }

            completionPanel.SetActive(true);
            wonLevel.text = "Level: " + (LevelSelection.Instance.currentLevel + 1).ToString();

            if (LevelSelection.Instance.totalUnlockedLevels <= LevelSelection.Instance.currentMaxLevels)
            {
                LevelSelection.Instance.totalUnlockedLevels += 1;
            }
        }
        else
        {
            Debug.Log("Not yet complete");
        }
    }

    private bool AllBottlesInValidState()
    {
        foreach (ColorChanger bottle in bottles)
        {
            if (!bottle.IsBottleInValidState())
                return false;
        }
        return true;
    }

    // UI Button Methods
    public void HomeButton()
    {
        PlayButtonSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelection");
    }

    public void Pause()
    {
        if (!isPaused)
        {
            PlaySoundEffect(pauseSound);
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            isPaused = true;
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            PlaySoundEffect(resumeSound);
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            isPaused = false;
        }
    }

    public void Restart()
    {
        PlayButtonSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Next()
    {
        PlayButtonSound();

        if (LevelSelection.Instance.currentLevel < LevelSelection.Instance.currentMaxLevels)
        {
            LevelSelection.Instance.currentLevel++;
            Level++;
            PersistentLevel = Level;
            PlayerPrefs.SetInt("UnlockedLevels", LevelSelection.Instance.totalUnlockedLevels);
            PlayerPrefs.Save();
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            SceneManager.LoadScene("LevelSelection");
        }
    }

    // Settings Methods
    public void ToggleSettings()
    {
        PlayButtonSound();

        if (settingsPanel == null) return;

        isSettingsOpen = !isSettingsOpen;
        settingsPanel.SetActive(isSettingsOpen);

        // Pause game when settings are open
        if (isSettingsOpen && !isPaused)
        {
            Time.timeScale = 0f;
        }
        else if (!isSettingsOpen && !pausePanel.activeInHierarchy)
        {
            Time.timeScale = 1f;
        }
    }

    private void ToggleSfx()
    {
        bool isSfxOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;
        isSfxOn = !isSfxOn;

        PlayerPrefs.SetInt("SfxOn", isSfxOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateSfxSprite(isSfxOn);

        // Apply to MusicManager
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSfxEnabled(isSfxOn);
        }

        // Play toggle sound (if SFX is being turned on)
        if (isSfxOn)
        {
            PlayButtonSound();
        }

        OnSfxToggled?.Invoke(isSfxOn);
        Debug.Log($"SFX turned {(isSfxOn ? "On" : "Off")}");
    }

    private void ToggleMusic()
    {
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isMusicOn = !isMusicOn;

        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateMusicSprite(isMusicOn);

        // Apply to MusicManager
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicEnabled(isMusicOn);
        }

        PlayButtonSound();
        OnMusicToggled?.Invoke(isMusicOn);
        Debug.Log($"Music turned {(isMusicOn ? "On" : "Off")}");
    }

    private void UpdateSfxSprite(bool isOn)
    {
        if (sfxImage != null && onSprite != null && offSprite != null)
        {
            sfxImage.sprite = isOn ? onSprite : offSprite;
        }
    }

    private void UpdateMusicSprite(bool isOn)
    {
        if (musicImage != null && onSprite != null && offSprite != null)
        {
            musicImage.sprite = isOn ? onSprite : offSprite;
        }
    }

    // Audio Helper Methods
    private void PlayButtonSound()
    {
        if (MusicManager.Instance != null && buttonClickSound != null)
        {
            MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        }
    }

    private void PlaySoundEffect(AudioClip clip, float volume = 0.7f)
    {
        if (MusicManager.Instance != null && clip != null)
        {
            MusicManager.Instance.PlaySFX(clip, volume);
        }
    }

    // Public methods for external access
    public void PlayCustomSFX(AudioClip clip, float volume = 1f)
    {
        PlaySoundEffect(clip, volume);
    }

    // Getters
    public bool IsMusicOn => PlayerPrefs.GetInt("MusicOn", 1) == 1;
    public bool IsSfxOn => PlayerPrefs.GetInt("SfxOn", 1) == 1;
    public bool IsGamePaused => isPaused;
    public bool IsSettingsOpen => isSettingsOpen;

    private void OnDestroy()
    {
        // Clean up time scale in case the scene is destroyed while paused
        Time.timeScale = 1f;
    }
}