using UnityEngine;

namespace Assets.Scripts
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private int damage = 1;

        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {

            Enemy enemy = other.GetComponentInParent<Enemy>();            
            if (enemy != null)
            {
                enemy.TakeHit(damage);
                Debug.Log("Bullet hit enemy");
            }

            Destroy(gameObject);
        }
    }
}