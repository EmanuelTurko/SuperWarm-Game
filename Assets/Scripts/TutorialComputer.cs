using UnityEngine;
using TMPro;

public class TutorialComputer : MonoBehaviour
{
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private string message = "This is text...";
    [SerializeField] private Light glowLight;

    private void Start()
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        if (glowLight != null)
            glowLight.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = message;

        if (glowLight != null)
            glowLight.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        if (glowLight != null)
            glowLight.enabled = true;
    }
}