using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Assets.Scripts;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject uiCanvas;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerStats playerStats;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Input")]
    [SerializeField] private InputActionReference ESCaction;

    private bool isPaused = false;
    public static bool IsPaused { get; private set; }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (uiCanvas != null)
            uiCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        if (ESCaction != null && ESCaction.action != null)
        {
            ESCaction.action.performed += OnPauseAction;
            ESCaction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (ESCaction != null && ESCaction.action != null)
        {
            ESCaction.action.performed -= OnPauseAction;
            ESCaction.action.Disable();
        }
    }

    private void OnPauseAction(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerStats != null)
            playerStats.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (uiCanvas != null)
            uiCanvas.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerStats != null)
            playerStats.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        isPaused = false;
        PauseMenu.IsPaused = false;

        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerStats != null)
            playerStats.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}