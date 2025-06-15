using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    public AudioClip pouringSound;
    public AudioClip[] backgroundMusic;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterBgmVolume = 0.8f;
    [Range(0f, 1f)]
    public float masterSfxVolume = 0.7f;
    [Range(0f, 1f)]
    public float pouringVolume = 0.7f;

    public static MusicManager Instance;

    private bool isMusicEnabled = true;
    private bool isSfxEnabled = true;
    private float originalBgmVolume;
    private float originalSfxVolume;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to menu manager events if it exists
        var menuManager = FindObjectOfType<MenuManager>();
        if (menuManager != null)
        {
            menuManager.OnMusicToggled += SetMusicEnabled;
            menuManager.OnSfxToggled += SetSfxEnabled;

            // Apply current settings from MenuManager
            SetMusicEnabled(menuManager.IsMusicOn);
            SetSfxEnabled(menuManager.IsSfxOn);
        }
        else
        {
            // Load settings from PlayerPrefs if no MenuManager found
            LoadAudioSettings();
        }
    }

    private void Initialize()
    {
        // Store original volumes
        if (bgmAudioSource != null)
            originalBgmVolume = bgmAudioSource.volume;

        if (sfxAudioSource != null)
            originalSfxVolume = sfxAudioSource.volume;

        // Set master volumes
        if (bgmAudioSource != null)
            bgmAudioSource.volume = masterBgmVolume;

        if (sfxAudioSource != null)
            sfxAudioSource.volume = masterSfxVolume;
    }

    private void LoadAudioSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSfxEnabled = PlayerPrefs.GetInt("SfxOn", 1) == 1;

        ApplyAudioSettings();
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        ApplyMusicSetting();
    }

    public void SetSfxEnabled(bool enabled)
    {
        isSfxEnabled = enabled;
        ApplySfxSetting();
    }

    private void ApplyAudioSettings()
    {
        ApplyMusicSetting();
        ApplySfxSetting();
    }

    private void ApplyMusicSetting()
    {
        if (bgmAudioSource != null)
        {
            if (isMusicEnabled)
            {
                bgmAudioSource.volume = masterBgmVolume;
                if (!bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
                {
                    bgmAudioSource.Play();
                }
            }
            else
            {
                bgmAudioSource.volume = 0f;
            }
        }
    }

    private void ApplySfxSetting()
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = isSfxEnabled ? masterSfxVolume : 0f;
        }
    }

    // Public methods for playing sounds
    public void PlayPouringSound()
    {
        if (pouringSound != null && isSfxEnabled)
        {
            PlaySFX(pouringSound, pouringVolume);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxAudioSource != null && clip != null && isSfxEnabled)
        {
            sfxAudioSource.PlayOneShot(clip, volume * masterSfxVolume);
        }
    }

    public void PlayBackgroundMusic(int musicIndex = 0)
    {
        if (bgmAudioSource != null && backgroundMusic != null &&
            musicIndex >= 0 && musicIndex < backgroundMusic.Length)
        {
            bgmAudioSource.clip = backgroundMusic[musicIndex];
            if (isMusicEnabled)
            {
                bgmAudioSource.Play();
            }
        }
    }

    public void StopBackgroundMusic()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    public void PauseBackgroundMusic()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Pause();
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (bgmAudioSource != null && isMusicEnabled)
        {
            bgmAudioSource.UnPause();
        }
    }

    // Volume control methods
    public void SetMasterBgmVolume(float volume)
    {
        masterBgmVolume = Mathf.Clamp01(volume);
        if (isMusicEnabled && bgmAudioSource != null)
        {
            bgmAudioSource.volume = masterBgmVolume;
        }
    }

    public void SetMasterSfxVolume(float volume)
    {
        masterSfxVolume = Mathf.Clamp01(volume);
        if (isSfxEnabled && sfxAudioSource != null)
        {
            sfxAudioSource.volume = masterSfxVolume;
        }
    }

    // Getters
    public bool IsMusicEnabled => isMusicEnabled;
    public bool IsSfxEnabled => isSfxEnabled;
    public float GetMasterBgmVolume() => masterBgmVolume;
    public float GetMasterSfxVolume() => masterSfxVolume;

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        var menuManager = FindObjectOfType<MenuManager>();
        if (menuManager != null)
        {
            menuManager.OnMusicToggled -= SetMusicEnabled;
            menuManager.OnSfxToggled -= SetSfxEnabled;
        }
    }
}