using UnityEngine;

namespace Assets.Scripts
{
    public class Enemy : MonoBehaviour
    {
        [Header("Health")]
        public int MaxHp = 3;
        public int CurrentHp;

        [Header("References")]
        public GameObject Visual;

        private bool _isDead = false;
        private Animator _animator;
        private float DeathWaitTimer = 0f;
        private EnemyShoot _enemyShoot;

        private PlayerStats _playerStats;

        
        [SerializeField] private GameObject deathEffectPrefab;
        private EnemyAI enemyAIscript;
   
        void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _enemyShoot = GetComponent<EnemyShoot>();
            enemyAIscript = GetComponent<EnemyAI>();

        }

        void Start()
        {
            CurrentHp = MaxHp;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerStats = player.GetComponent<PlayerStats>();
        }

        public void TakeHit(int damage = 1)
        {
            if (_isDead) return;

            if (_animator != null)
            {
                // Enter damage state
                _animator.SetBool("Damage02", true);
                _animator.SetBool("Hit", false);
            }

            CurrentHp -= damage;

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                if (_enemyShoot != null) _enemyShoot.enabled = false;
                Die();
                return;
            }
            StartCoroutine(ClearDamageAfterDelay());

         
        }

        private System.Collections.IEnumerator ClearDamageAfterDelay()
        {
            yield return new WaitForSeconds(0.16f); // match damage clip length

            if (_animator != null)
            {
                _animator.SetBool("Damage02", false);
                _animator.SetBool("Hit", true); // tells Animator to go back to None
            }
        }

        private void Die()
        {
            
            _isDead = true;
            enemyAIscript.enabled = false;
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            
            }
            //Destroy(gameObject);

            if (_animator != null)
            {
                // First, clear all death flags
                _animator.SetBool("Death01", false);
                _animator.SetBool("Death02", false);
                _animator.SetBool("Death03", false);


                // Pick a random one
                int deathIndex = Random.Range(1, 4); // 1..5
                string paramName = $"Death0{deathIndex}";
                _animator.SetBool(paramName, true);
                
                if (deathEffectPrefab != null)
                {
                    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                }

                
                switch (deathIndex)
                {
                    case 1:
                        DeathWaitTimer = 0.733f;
                        break;
                    case 2:
                        DeathWaitTimer = 0.667f;
                        break;
                    case 3:
                        DeathWaitTimer = 1.567f;
                        break;
                }
             }

                if (_playerStats != null)
                    _playerStats.AddTime(5f);

            // Optionally, disable movement scripts here, but keep visual active
            var patrol = GetComponent<EnemyPatrol>();
            if (patrol != null)
                patrol.enabled = false;

            // Hide / disable after short delay to let animation play
            StartCoroutine(HandleDeathCleanup());
        }

        private System.Collections.IEnumerator HandleDeathCleanup()
        {
            // Let the death animation play once; you can keep this short (e.g. 1.2f)
            yield return new WaitForSeconds(DeathWaitTimer); // slightly longer than the longest death clip

            if (_animator != null)
            {
                _animator.SetBool("Dead", true);  // tells Animator to go to a non-animating dead state
                _animator.enabled = false;  
            }


            if (Visual != null)
                Visual.SetActive(false);

            var coll = GetComponent<Collider>();
            if (coll != null)
                coll.enabled = false;

            Destroy(gameObject, 1f);
        }
    }
}