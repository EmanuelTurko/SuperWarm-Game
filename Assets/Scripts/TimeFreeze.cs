using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class TimeFreeze : MonoBehaviour
    {
        [SerializeField] private float activeTimeScale = 1f;
        [SerializeField] private float idleTimeScale = 0.05f;
        [SerializeField] private float mouseThreshold = 0.01f;

        private float _defaultFixedDeltaTime;

        private void Start()
        {
            _defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        public void SetDefaultValues()
        {
            activeTimeScale = 1f;
            idleTimeScale = 1f;
            mouseThreshold = 1f;
        }

        public void SetGameValues()
        {
            activeTimeScale = 1f;
            idleTimeScale = 0.05f;
            mouseThreshold = 0.01f;
        }

        private void Update()
        {
            if (PauseMenu.IsPaused)
            {
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0f;
                return;
            }

            bool hasMovementInput =
                Keyboard.current != null &&
                (
                    Keyboard.current.wKey.isPressed ||
                    Keyboard.current.aKey.isPressed ||
                    Keyboard.current.sKey.isPressed ||
                    Keyboard.current.dKey.isPressed
                );

            bool hasJumpInput = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            bool hasShootInput = Mouse.current != null && Mouse.current.leftButton.isPressed;

            Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            bool hasMouseInput = mouseDelta.sqrMagnitude > mouseThreshold;

            float targetTimeScale =
                (hasMovementInput || hasJumpInput || hasShootInput || hasMouseInput)
                    ? activeTimeScale
                    : idleTimeScale;

            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _defaultFixedDeltaTime;
        }
    }
}