using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject menuPausePanel;

    [Header("Panels")]
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle windowedModeToggle;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 1f;

    // Track the pause state
    private bool isPaused = false;

    // References to menu button hover controllers
    private MenuButtonHoverController[] menuButtonControllers;

    private void Awake()
    {
        // Ensure all panels are hidden on startup
        pauseMenuRoot?.SetActive(false);
        if (menuPausePanel) menuPausePanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // Cache references to all menu button hover controllers
        if (menuPausePanel)
        {
            menuButtonControllers = menuPausePanel.GetComponentsInChildren<MenuButtonHoverController>(true);
        }

        // Initialize settings controls
        InitializeSettingsControls();
    }

    private void InitializeSettingsControls()
    {
        // Initialize volume slider
        if (volumeSlider)
        {
            volumeSlider.value = masterVolume;
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // Initialize music slider
        if (musicSlider)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Initialize windowed mode toggle
        if (windowedModeToggle)
        {
            windowedModeToggle.isOn = !Screen.fullScreen;
            windowedModeToggle.onValueChanged.AddListener(SetWindowedMode);
        }
    }

    private void Update()
    {
        // Toggle pause menu when the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause the game but not the UI
            Time.timeScale = 0f;

            // Show main pause panel
            ShowPausePanel();
        }
        else
        {
            // Hide all panels
            HideAllPanels(true);

            // Resume the game
            Time.timeScale = 1f;
        }
    }

    // Show main pause panel
    private void ShowPausePanel()
    {
        // Hide all panels first
        HideAllPanels();

        // Show only main pause panel
        pauseMenuRoot?.SetActive(true);
        if (menuPausePanel) menuPausePanel.SetActive(true);

        // Reset all hover states when returning to main menu
        ResetAllButtonHoverStates();
    }

    // Show restart panel
    public void ShowRestartPanel()
    {
        HideAllPanels();
        if (restartPanel) restartPanel.SetActive(true);
    }

    // Show controls panel
    public void ShowControlsPanel()
    {
        HideAllPanels();
        if (controlsPanel) controlsPanel.SetActive(true);
    }

    // Show settings panel
    public void ShowSettingsPanel()
    {
        HideAllPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    // Reset all hover states on menu buttons
    private void ResetAllButtonHoverStates()
    {
        if (menuButtonControllers != null)
        {
            foreach (var controller in menuButtonControllers)
            {
                if (controller != null && controller.gameObject.activeInHierarchy)
                {
                    controller.ForceResetHoverState();
                }
            }
        }
    }

    // Hide all panels
    private void HideAllPanels(bool hideRoot = false)
    {
        if (menuPausePanel) menuPausePanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (hideRoot)
        {
            pauseMenuRoot?.SetActive(false);
        }
    }

    // Resume game (can be called by UI button)
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePauseMenu();
        }
    }

    // Restart game (can be called by UI button)
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Reset pause state
        isPaused = false;

        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // Decline restart and return to pause menu
    public void DeclineRestart()
    {
        ShowPausePanel();
    }

    // Quit game (can be called by UI button)
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Return to main pause menu (can be called by UI button)
    public void ReturnToMainPauseMenu()
    {
        ShowPausePanel();
    }

    // Set master volume
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        AudioListener.volume = volume;
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource)
        {
            musicSource.volume = volume;
        }
    }

    // Toggle between fullscreen and windowed mode
    public void SetWindowedMode(bool isWindowed)
    {
        Screen.fullScreen = !isWindowed;

        // Save player preference
        PlayerPrefs.SetInt("WindowedMode", isWindowed ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Save all settings
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetInt("WindowedMode", windowedModeToggle && windowedModeToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        // Return to main pause menu
        ShowPausePanel();
    }

    // Load saved settings
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            if (volumeSlider) volumeSlider.value = masterVolume;
            AudioListener.volume = masterVolume;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            if (musicSlider) musicSlider.value = musicVolume;
            if (musicSource) musicSource.volume = musicVolume;
        }

        if (PlayerPrefs.HasKey("WindowedMode"))
        {
            bool isWindowed = PlayerPrefs.GetInt("WindowedMode") == 1;
            if (windowedModeToggle) windowedModeToggle.isOn = isWindowed;
            Screen.fullScreen = !isWindowed;
        }
    }
}