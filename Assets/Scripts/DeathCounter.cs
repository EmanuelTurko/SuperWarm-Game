using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathCounter : MonoBehaviour
{
    public static DeathCounter Instance { get; private set; }

    public int TotalDeaths { get; private set; }

    private bool runStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            ResetCounter();
            runStarted = false;
            return;
        }

        if (scene.name == "Scene1")
        {
            runStarted = true;
            ResetCounter();
        }

        if (scene.name == "Victory")
        {
            runStarted = false;
        }
    }

    public void AddDeath()
    {
        if (!runStarted)
            return;

        TotalDeaths++;
    }

    public void ResetCounter()
    {
        TotalDeaths = 0;
    }
}