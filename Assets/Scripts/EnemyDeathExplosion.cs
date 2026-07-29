using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyDeathExplosion : MonoBehaviour
    {
        [SerializeField] private GameObject fracturedEnemyPrefab;
        [SerializeField] private float destroyAfter = 3f;
        [SerializeField] private float explosionForce = 300f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float upwardModifier = 0.5f;

        public void Die()
        {
            if (fracturedEnemyPrefab != null)
            {
                GameObject broken = Instantiate(
                    fracturedEnemyPrefab,
                    transform.position,
                    transform.rotation
                );

                Rigidbody[] bodies = broken.GetComponentsInChildren<Rigidbody>();

                foreach (Rigidbody rb in bodies)
                {
                    rb.AddExplosionForce(
                        explosionForce,
                        transform.position,
                        explosionRadius,
                        upwardModifier,
                        ForceMode.Impulse
                    );
                }

                Destroy(broken, destroyAfter);
            }

            Destroy(gameObject);
        }
    }
}