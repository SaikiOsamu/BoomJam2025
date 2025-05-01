using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPausePanel;
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Button Components")]
    [SerializeField] private GameObject restartBTN;
    [SerializeField] private GameObject controlsBTN;
    [SerializeField] private GameObject settingsBTN;
    [SerializeField] private GameObject quitBTN;

    [Header("Return Buttons")]
    [SerializeField] private GameObject restartReturnBTN;
    [SerializeField] private GameObject controlsReturnBTN;
    [SerializeField] private GameObject settingsReturnBTN;

    [Header("Yes/No Buttons")]
    [SerializeField] private GameObject restartYesBTN;
    [SerializeField] private GameObject restartNoBTN;

    // Keep track of game's paused state
    private bool isPaused = false;

    // References to hover controllers
    private List<TriangleButtonHover> triangleHovers = new List<TriangleButtonHover>();
    private List<MenuButtonHoverController> menuHovers = new List<MenuButtonHoverController>();

    private void Awake()
    {
        // Make sure all panels are initially hidden
        if (menuPausePanel) menuPausePanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // Collect all hover controllers for later use
        CollectHoverControllers();
    }

    private void CollectHoverControllers()
    {
        // Find all Triangle and Menu button hover controllers in our panels
        if (menuPausePanel)
        {
            triangleHovers.AddRange(menuPausePanel.GetComponentsInChildren<TriangleButtonHover>(true));
            menuHovers.AddRange(menuPausePanel.GetComponentsInChildren<MenuButtonHoverController>(true));
        }

        if (restartPanel)
        {
            triangleHovers.AddRange(restartPanel.GetComponentsInChildren<TriangleButtonHover>(true));
            menuHovers.AddRange(restartPanel.GetComponentsInChildren<MenuButtonHoverController>(true));
        }

        if (controlsPanel)
        {
            triangleHovers.AddRange(controlsPanel.GetComponentsInChildren<TriangleButtonHover>(true));
            menuHovers.AddRange(controlsPanel.GetComponentsInChildren<MenuButtonHoverController>(true));
        }

        if (settingsPanel)
        {
            triangleHovers.AddRange(settingsPanel.GetComponentsInChildren<TriangleButtonHover>(true));
            menuHovers.AddRange(settingsPanel.GetComponentsInChildren<MenuButtonHoverController>(true));
        }
    }

    private void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause the game and show main pause menu
            Time.timeScale = 0f;
            ShowMainPausePanel();
        }
        else
        {
            // Resume the game and hide all panels
            Time.timeScale = 1f;
            HideAllPanels();
        }
    }

    // Button handlers for main pause menu
    public void OnRestartButtonClicked()
    {
        ShowPanel(restartPanel);
    }

    public void OnControlsButtonClicked()
    {
        ShowPanel(controlsPanel);
    }

    public void OnSettingsButtonClicked()
    {
        ShowPanel(settingsPanel);
    }

    public void OnQuitButtonClicked()
    {
        // Quit the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // Restart panel button handlers
    public void OnRestartYesButtonClicked()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Restart the current level
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void OnRestartNoButtonClicked()
    {
        // Return to main pause menu
        ShowMainPausePanel();
    }

    // Return button handler (for controls and settings panels)
    public void OnReturnButtonClicked()
    {
        // Return to main pause menu
        ShowMainPausePanel();
    }

    // Panel visibility management
    private void ShowMainPausePanel()
    {
        // Hide all panels first
        HideAllPanels();

        // Show only the main pause menu
        if (menuPausePanel)
        {
            menuPausePanel.SetActive(true);

            // Ensure all hover components are properly initialized
            StartCoroutine(InitializeHoverComponents(menuPausePanel));
        }
    }

    private void ShowPanel(GameObject panel)
    {
        // Hide the main pause menu
        if (menuPausePanel) menuPausePanel.SetActive(false);

        // Show the requested panel
        if (panel)
        {
            panel.SetActive(true);

            // Ensure all hover components are properly initialized
            StartCoroutine(InitializeHoverComponents(panel));
        }
    }

    private void HideAllPanels()
    {
        // Hide all menu panels
        if (menuPausePanel) menuPausePanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // This ensures that when a panel is activated, all hover controllers are properly initialized
    private IEnumerator InitializeHoverComponents(GameObject panel)
    {
        // Wait one frame to make sure the panel is fully activated
        yield return null;

        // Re-fetch the components in case they've changed
        TriangleButtonHover[] triangles = panel.GetComponentsInChildren<TriangleButtonHover>(true);
        MenuButtonHoverController[] menus = panel.GetComponentsInChildren<MenuButtonHoverController>(true);

        // For TriangleButtonHover components, make sure they have the correct references
        foreach (TriangleButtonHover hover in triangles)
        {
            // Ensure the component is properly initialized
            if (hover.gameObject.activeInHierarchy)
            {
                // Force component to refresh its internal state by toggling active state
                // This is a common Unity trick to ensure components correctly initialize
                hover.gameObject.SetActive(false);
                hover.gameObject.SetActive(true);
            }
        }

        // For MenuButtonHoverController components, do the same
        foreach (MenuButtonHoverController hover in menus)
        {
            if (hover.gameObject.activeInHierarchy)
            {
                hover.gameObject.SetActive(false);
                hover.gameObject.SetActive(true);
            }
        }
    }

    // Helper method to set up button click events in the Unity Editor
    public void SetupButtonEvents()
    {
        // This method would be called from the editor script or manually
        // to set up all the button click events

        // Example of how we might set up a button:
        /*
        Button restartButton = restartBTN.GetComponent<Button>();
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        */
    }
}