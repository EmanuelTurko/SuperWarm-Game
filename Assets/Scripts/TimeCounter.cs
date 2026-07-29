using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeCounter : MonoBehaviour
{
    public static TimeCounter Instance { get; private set; }

    public float TotalTime { get; private set; }

    private bool runStarted;
    private bool counting;

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

    private void Update()
    {
        if (counting)
            TotalTime += Time.deltaTime;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            ResetTimer();
            runStarted = false;
            counting = false;
            return;
        }

        if (scene.name == "Tutorial")
        {
            runStarted = true;
            counting = true;
            ResetTimer();
            return;
        }

        if (scene.name == "Level1")
        {
            counting = runStarted;
            return;
        }

        if (scene.name == "Level2")
        {
            counting = runStarted;
            return;
        }

        if (scene.name == "Victory")
        {
            counting = false;
        }
    }

    public void ResetTimer()
    {
        TotalTime = 0f;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(TotalTime / 60f);
        int seconds = Mathf.FloorToInt(TotalTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}