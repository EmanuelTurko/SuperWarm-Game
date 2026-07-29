using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        public int MaxHp = 2;
        public int CurrentHp;

        [Header("Timer")]
        public float LevelTime = 60f;
        public float TimeRemaining;
        public float TimePerKill = 5f;

        [Header("UI")]
        public Slider HpBar;
        public TMP_Text TimeText;
        public Image TimeRadial;

        void Start()
        {
            CurrentHp = MaxHp;
            TimeRemaining = LevelTime;
            UpdateHpUi();
            UpdateTimeUi();
        }

        void Update()
        {
            UpdateTimer();
        }

        public void TakeHit()
        {
            CurrentHp -= 1;
            if (CurrentHp < 0) CurrentHp = 0;

            UpdateHpUi();

            DamageFeedback feedback = FindFirstObjectByType<DamageFeedback>();
            if (feedback != null)
                feedback.PlayHitFeedback();

            if (CurrentHp == 0)
                HandleDeath();
        }

        public void AddTime(float amount)
        {
            TimeRemaining = Mathf.Min(TimeRemaining + amount,LevelTime);

            if (TimeRemaining > LevelTime)
                TimeRemaining = LevelTime;

            UpdateTimeUi();
        }
        private void UpdateTimer()
        {
            if (TimeRemaining <= 0f) return;

            TimeRemaining -= Time.deltaTime;

            if (TimeRemaining < 0) TimeRemaining = 0;
            
            UpdateTimeUi();
            
            if(TimeRemaining == 0f) HandleDeath();
        }

        private void UpdateHpUi()
        {
            if (HpBar != null)
            {
                HpBar.maxValue = MaxHp;
                HpBar.value = CurrentHp;
            }
        }

        private void UpdateTimeUi()
        {
            if (TimeText != null) TimeText.text = $"Time: {TimeRemaining:F1}";
            
            if (TimeRadial != null)
            {
                float ratio = TimeRemaining / LevelTime;
                TimeRadial.fillAmount = ratio;

                if (ratio > 0.5f)
                    TimeRadial.color = Color.white;
                else if (ratio > 0.2f)
                    TimeRadial.color = Color.yellow;
                else
                    TimeRadial.color = Color.red;
            }
        }

        private void HandleDeath()
        {
            DeathCounter.Instance?.AddDeath();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
    }
}
