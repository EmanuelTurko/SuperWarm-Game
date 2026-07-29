using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class ScenePortal : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            SceneManager.LoadScene(sceneToLoad);
        }
    }
}