using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;

        private Rigidbody rb;
        private Vector3 savedVelocity;
        private Vector3 savedAngularVelocity;
        private bool wasPaused;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            if (PauseMenu.IsPaused && !wasPaused)
            {
                PauseProjectile();
            }
            else if (!PauseMenu.IsPaused && wasPaused)
            {
                ResumeProjectile();
            }
        }

        private void PauseProjectile()
        {
            wasPaused = true;

            if (rb == null)
                return;

#if UNITY_6000_0_OR_NEWER
            savedVelocity = rb.linearVelocity;
#else
            savedVelocity = rb.velocity;
#endif
            savedAngularVelocity = rb.angularVelocity;

            rb.isKinematic = true;
        }

        private void ResumeProjectile()
        {
            wasPaused = false;

            if (rb == null)
                return;

            rb.isKinematic = false;
            rb.WakeUp();

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = savedVelocity;
#else
            rb.velocity = savedVelocity;
#endif
            rb.angularVelocity = savedAngularVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PauseMenu.IsPaused)
                return;

            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeHit();
                Debug.Log("Player took hit");
                Destroy(gameObject);
                return;
            }

            if (!other.isTrigger)
                Destroy(gameObject);
        }
    }
}