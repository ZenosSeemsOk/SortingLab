using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private Button sfxToggle;
    [SerializeField] private Button musicToggle;

    [Header("Toggle Sprites")]
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Settings")]
    [SerializeField] private bool startWithMusicOn = true;
    [SerializeField] private bool startWithSfxOn = true;

    private Image musicImage;
    private Image sfxImage;
    private bool isSettingsOpen;
    private bool isMusicOn;
    private bool isSfxOn;


    [SerializeField] private AudioClip buttonClickSound;

    // Events for other systems to subscribe to
    public System.Action<bool> OnMusicToggled;
    public System.Action<bool> OnSfxToggled;

    private void Awake()
    {
        // Cache Image components (UI buttons typically use Image, not SpriteRenderer)
        musicImage = musicToggle.GetComponent<Image>();
        sfxImage = sfxToggle.GetComponent<Image>();

        // Fallback to SpriteRenderer if Image is not found
        if (musicImage == null && musicToggle.TryGetComponent<SpriteRenderer>(out var musicSpriteRenderer))
            musicImage = null; // Will be handled separately
        if (sfxImage == null && sfxToggle.TryGetComponent<SpriteRenderer>(out var sfxSpriteRenderer))
            sfxImage = null; // Will be handled separately
    }

    private void Start()
    {
        InitializeSettings();
        SetupButtonListeners();
        ConnectToMusicManager();
    }

    private void InitializeSettings()
    {
        // Load saved settings from PlayerPrefs
        isMusicOn = PlayerPrefs.GetInt("MusicOn", startWithMusicOn ? 1 : 0) == 1;
        isSfxOn = PlayerPrefs.GetInt("SfxOn", startWithSfxOn ? 1 : 0) == 1;

        // Update UI to reflect current state
        UpdateMusicSprite();
        UpdateSfxSprite();

        // Ensure settings panel starts closed
        if (settingsUI != null)
        {
            settingsUI.SetActive(false);
            isSettingsOpen = false;
        }
    }

    private void ConnectToMusicManager()
    {
        // Apply current settings to MusicManager if it exists
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicEnabled(isMusicOn);
            MusicManager.Instance.SetSfxEnabled(isSfxOn);
        }
    }

    private void SetupButtonListeners()
    {
        if (sfxToggle != null)
            sfxToggle.onClick.AddListener(ToggleSfx);
        if (musicToggle != null)
            musicToggle.onClick.AddListener(ToggleMusic);
    }

    public void OnPlayEasy()
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        SceneManager.LoadScene("LevelSelection");
    }

    public void ToggleSettings()
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        if (settingsUI == null) return;

        isSettingsOpen = !isSettingsOpen;
        settingsUI.SetActive(isSettingsOpen);
    }

    private void ToggleSfx()
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        isSfxOn = !isSfxOn;
        UpdateSfxSprite();
        SaveSettings();

        // Apply to MusicManager immediately
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSfxEnabled(isSfxOn);
        }

        // Notify other systems
        OnSfxToggled?.Invoke(isSfxOn);

        Debug.Log($"SFX turned {(isSfxOn ? "On" : "Off")}");
    }

    private void ToggleMusic()
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        isMusicOn = !isMusicOn;
        UpdateMusicSprite();
        SaveSettings();

        // Apply to MusicManager immediately
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicEnabled(isMusicOn);
        }

        // Notify other systems
        OnMusicToggled?.Invoke(isMusicOn);

        Debug.Log($"Music turned {(isMusicOn ? "On" : "Off")}");
    }

    private void UpdateSfxSprite()
    {
        if (sfxImage != null)
            sfxImage.sprite = isSfxOn ? sfxOnSprite : sfxOffSprite;
        else if (sfxToggle.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.sprite = isSfxOn ? sfxOnSprite : sfxOffSprite;
    }

    private void UpdateMusicSprite()
    {
        if (musicImage != null)
            musicImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
        else if (musicToggle.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("SfxOn", isSfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Public getters for other systems to check current state
    public bool IsMusicOn => isMusicOn;
    public bool IsSfxOn => isSfxOn;

    // Method to programmatically set states (useful for loading game settings)
    public void SetMusicState(bool enabled)
    {
        isMusicOn = enabled;
        UpdateMusicSprite();
        SaveSettings();

        // Apply to MusicManager immediately
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicEnabled(isMusicOn);
        }

        OnMusicToggled?.Invoke(isMusicOn);
    }

    public void SetSfxState(bool enabled)
    {
        isSfxOn = enabled;
        UpdateSfxSprite();
        SaveSettings();

        // Apply to MusicManager immediately
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetSfxEnabled(isSfxOn);
        }

        OnSfxToggled?.Invoke(isSfxOn);
    }
}