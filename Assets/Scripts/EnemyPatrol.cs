using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyPatrol : MonoBehaviour
    {
        public Transform[] PatrolPoints;
        public float MoveSpeed = 2f;
        public float WaitTimeAtPoint = 2f;

        private int _currentIndex = 0;
        private float _waitTimer = 0f;

        private Animator _animator;

        void Awake()
        {
            // Get Animator from child (HumanCharacterDummy_M)
            _animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            if (PatrolPoints == null || PatrolPoints.Length == 0)
            {
                PatrolPoints = new Transform[1];
                var dummy = new GameObject("PatrolPoint_Auto").transform;
                dummy.position = transform.position;
                PatrolPoints[0] = dummy;
            }

            // All enemies have rifle
            if (_animator != null)
            {
                _animator.SetBool("Rifle", true);

                // If no patrol points: idle/no-movement animation
                if (PatrolPoints == null || PatrolPoints.Length == 0)
                {
                    _animator.SetBool("Walk", false);
                    _animator.SetBool("NoMovement", true);
                }
            }
        }

        void Update()
        {
            // If no patrol points, do nothing (enemy just stands and can shoot)
            if (PatrolPoints == null || PatrolPoints.Length == 0)
            {
                return;
            }

            Transform targetPoint = PatrolPoints[_currentIndex];
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPoint.position);

            bool isMoving = distance >= 0.1f;

            if (isMoving)
            {
                transform.position += direction * MoveSpeed * Time.deltaTime;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 5f);
                }
            }
            else
            {
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= WaitTimeAtPoint)
                {
                    _waitTimer = 0f;
                    _currentIndex = (_currentIndex + 1) % PatrolPoints.Length;
                }
            }

            if (_animator != null)
            {
                _animator.SetBool("Walk", isMoving);
                _animator.SetBool("NoMovement", !isMoving);
            }
        }
    }
}