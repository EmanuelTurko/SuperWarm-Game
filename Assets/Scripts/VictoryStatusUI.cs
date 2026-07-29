using TMPro;
using UnityEngine;

public class VictoryStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        if (deathsText != null)
            deathsText.text = "Deaths: " + (DeathCounter.Instance != null ? DeathCounter.Instance.TotalDeaths : 0);

        if (timeText != null)
            timeText.text = "Time: " + (TimeCounter.Instance != null ? TimeCounter.Instance.GetFormattedTime() : "00:00");
    }
}