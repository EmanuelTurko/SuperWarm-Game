using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class PlayerMovement : MonoBehaviour
    {
        public float MoveSpeed = 5f;
        public float LookSensitivity = 0.1f;
        public float Gravity = -9.81f;
        public float MaxShootDistance = 100f;
        public LayerMask ShootLayerMask;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private InputActionReference toggleCameraAction;

        private Animator _animator;

        [SerializeField] private RectTransform crosshairRect;

        [SerializeField] private Transform spineBone;
        [SerializeField] private float spinePitchMultiplier = 1f;
        [SerializeField] private float spinePitchOffset = 0f;

        [SerializeField] private float fpsminPitch;
        [SerializeField] private float fpsmaxPitch;

        private Quaternion _spineStartLocalRotation;

        private AudioListener _fpsListener;
        private AudioListener _tpsListener;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private float projectileSpeed = 40f;

        [SerializeField] private float fireCooldown = 0.25f;
        [SerializeField] private int maxActiveBullets = 3;

        private float _nextFireTime = 0f;
        private readonly List<GameObject> _activeBullets = new List<GameObject>();

        private CharacterController _controller;
        private Camera _playerCamera;
        [SerializeField] private Camera _fpsCamera;
        [SerializeField] private Camera _tpsCamera;

        private Vector3 _velocity;
        private float _cameraPitch;
        private bool isThirdPerson = false;

        private void OnEnable()
        {
            moveAction?.action.Enable();
            lookAction?.action.Enable();
            fireAction?.action.Enable();
            toggleCameraAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            lookAction?.action.Disable();
            fireAction?.action.Disable();
            toggleCameraAction?.action.Disable();
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _playerCamera = _fpsCamera;
            _animator = GetComponentInChildren<Animator>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (spineBone != null)
                _spineStartLocalRotation = spineBone.localRotation;

            if (_fpsCamera != null) _fpsCamera.gameObject.SetActive(true);
            if (_tpsCamera != null) _tpsCamera.gameObject.SetActive(false);

            if (_animator != null)
            {
                _animator.SetBool("Gun", true);
                _animator.SetBool("Aim", true);
                _animator.SetBool("WalkW", false);
                _animator.SetBool("WalkS", false);
            }

            if (_fpsCamera != null) _fpsListener = _fpsCamera.GetComponent<AudioListener>();
            if (_tpsCamera != null) _tpsListener = _tpsCamera.GetComponent<AudioListener>();

            if (_fpsListener != null) _fpsListener.enabled = true;
            if (_tpsListener != null) _tpsListener.enabled = false;
        }

        private void Update()
        {
            if (PauseMenu.IsPaused)
                return;

            HandleLook();
            HandleMovement();
            HandleShooting();
            HandleCameraToggle();
        }

        private void LateUpdate()
        {
            UpdateSpinePitch();
        }

        private void UpdateSpinePitch()
        {
            if (spineBone == null)
                return;

            float pitch = _cameraPitch * spinePitchMultiplier + spinePitchOffset;

            spineBone.localRotation =
                _spineStartLocalRotation *
                Quaternion.AngleAxis(pitch, Vector3.right);
        }

        private void HandleLook()
        {
            if (_playerCamera == null || lookAction == null)
                return;

            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            float mouseX = lookInput.x * LookSensitivity;
            float mouseY = lookInput.y * LookSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            float minPitch = isThirdPerson ? -20f : fpsminPitch;
            float maxPitch = isThirdPerson ? 20f : fpsmaxPitch;

            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);
            _playerCamera.transform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (moveAction == null || _controller == null)
                return;

            Vector2 input = moveAction.action.ReadValue<Vector2>();

            float moveX = input.x;
            float moveZ = input.y;

            bool wPressed = moveZ > 0.1f;
            bool sPressed = moveZ < -0.1f;

            UpdateAnimationState(wPressed, sPressed);

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            move = Vector3.ClampMagnitude(move, 1f) * MoveSpeed;

            if (_controller.isGrounded && _velocity.y < 0f)
                _velocity.y = 0f;

            _velocity.y += Gravity * Time.deltaTime;

            Vector3 finalMove = move + Vector3.up * _velocity.y;
            _controller.Move(finalMove * Time.deltaTime);
        }

        private void UpdateAnimationState(bool wPressed, bool sPressed)
        {
            if (_animator == null)
                return;

            if (isThirdPerson)
            {
                _animator.SetBool("WalkW", wPressed);
                _animator.SetBool("WalkS", sPressed);
            }
            else
            {
                _animator.SetBool("WalkW", false);
                _animator.SetBool("WalkS", false);
            }
        }

        private void HandleShooting()
        {
            if (_playerCamera == null || fireAction == null)
                return;

            _activeBullets.RemoveAll(b => b == null || !b.activeInHierarchy);

            if (!fireAction.action.WasPressedThisFrame())
                return;

            if (Time.time < _nextFireTime)
                return;

            if (_activeBullets.Count >= maxActiveBullets)
                return;

            _nextFireTime = Time.time + fireCooldown;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = _playerCamera.ScreenPointToRay(screenCenter);

            Debug.DrawRay(ray.origin, ray.direction * MaxShootDistance, Color.red, 1f);

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, MaxShootDistance, ShootLayerMask))
                targetPoint = hit.point;
            else
                targetPoint = ray.origin + ray.direction * MaxShootDistance;

            if (projectilePrefab != null && muzzlePoint != null)
            {
                Vector3 shootDirection = (targetPoint - muzzlePoint.position).normalized;

                GameObject projectile = Instantiate(
                    projectilePrefab,
                    muzzlePoint.position,
                    Quaternion.LookRotation(shootDirection)
                );

                _activeBullets.Add(projectile);

                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = false;
                    rb.linearVelocity = shootDirection * projectileSpeed;
                    rb.WakeUp();
                }
            }
        }

        private void HandleCameraToggle()
        {
            if (toggleCameraAction == null || !toggleCameraAction.action.WasPressedThisFrame())
                return;

            if (_fpsCamera == null || _tpsCamera == null)
                return;

            isThirdPerson = !isThirdPerson;

            if (_animator != null && !isThirdPerson)
            {
                _animator.SetBool("WalkW", false);
                _animator.SetBool("WalkS", false);
            }

            if (isThirdPerson)
            {
                _fpsCamera.gameObject.SetActive(false);
                _tpsCamera.gameObject.SetActive(true);
                _playerCamera = _tpsCamera;

                if (_fpsListener != null) _fpsListener.enabled = false;
                if (_tpsListener != null) _tpsListener.enabled = true;
            }
            else
            {
                _tpsCamera.gameObject.SetActive(false);
                _fpsCamera.gameObject.SetActive(true);
                _playerCamera = _fpsCamera;

                if (_tpsListener != null) _tpsListener.enabled = false;
                if (_fpsListener != null) _fpsListener.enabled = true;
            }
        }
    }
}