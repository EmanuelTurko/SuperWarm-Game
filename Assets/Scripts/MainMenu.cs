using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu UI")]
        [SerializeField] private GameObject titleText;
        [SerializeField] private GameObject playButton;
        [SerializeField] private GameObject quitButton;
        [SerializeField] private GameObject loadingText;
        [SerializeField] private GameObject loadingPanel;

        [Header("Scene")]
        [SerializeField] private string gameplaySceneName = "Tutorial";
        [SerializeField] private float fakeLoadTime = 1.5f;

        private bool isLoading = false;

        void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (loadingText != null)
                loadingText.SetActive(false);
            
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public void StartGame()
        {
            if (isLoading) return;
            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            isLoading = true;

            if (titleText != null)
                titleText.SetActive(false);

            if (playButton != null)
                playButton.SetActive(false);

            if (quitButton != null)
                quitButton.SetActive(false);

            if(loadingPanel != null) loadingPanel.SetActive(true);
            if (loadingText != null)
                loadingText.SetActive(true);

            yield return new WaitForSeconds(fakeLoadTime);

            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}