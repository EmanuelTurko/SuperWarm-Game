using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        private enum State
        {
            PatrolMove,
            PatrolScan,
            Combat,
            Investigate
        }

        private enum EnemyMode
        {
            Normal,
            CombatOnly
        }

        [Header("Mode")]
        [SerializeField] private EnemyMode enemyMode = EnemyMode.Normal;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform eyePoint;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float maxProjectileBlockTime = 1f;
        [SerializeField] private ParticleSystem muzzleFlashPrefab;
        
        private float activeProjectileSpawnTime;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private bool useRandomPatrolIfNoPoints = true;
        [SerializeField] private float randomPatrolRadius = 8f;
        [SerializeField] private float patrolStoppingDistance = 0.35f;

        [Header("Scan")]
        [SerializeField] private float scanTurnAngle = 60f;
        [SerializeField] private float scanTurnSpeed = 180f;
        [SerializeField] private float scanPauseTime = 0.2f;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float fieldOfViewAngle = 120f;
        [SerializeField] private float eyeHeightOffset = 1.5f;
        [SerializeField] private float targetHeightOffset = 1.2f;
        [SerializeField] private LayerMask visionMask;
        [SerializeField] private float investigateDelay = 1.2f;

        [Header("Combat")]
        [SerializeField] private float aimTurnSpeed = 10f;
        [SerializeField] private float fireCooldown = 0.8f;
        [SerializeField] private float muzzleAimTolerance = 10f;
        [SerializeField] private float projectileSpeed = 12f;

        [Header("Investigate")]
        [SerializeField] private float investigateStoppingDistance = 0.5f;
        [SerializeField] private float investigateWaitTime = 1f;

        [Header("Extras")]
        [SerializeField] private GameObject exclamationMark;
        [SerializeField] private GameObject questionMark;
        [SerializeField] private float indicatorDuration = 0.5f;

        private Coroutine exclamationRoutine;
        private Coroutine questionRoutine;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private NavMeshAgent agent;
        private State currentState;

        private int patrolIndex;
        private float nextFireTime;
        private float lostSightTimer;
        private float investigateTimer;
        private float scanPauseTimer;

        private Quaternion scanCenterRotation;
        private Quaternion scanLeftRotation;
        private Quaternion scanRightRotation;
        private int scanPhase;

        private bool hasLastKnownPosition;
        private Vector3 lastKnownPlayerPosition;

        private GameObject activeProjectile;

        private bool debugVisible;
        private bool debugInRange;
        private bool debugInFOV;
        private bool debugRayHitSomething;
        private string debugHitName = "";
        private Vector3 debugEyePos;
        private Vector3 debugTargetPos;
        private Vector3 debugRayDir;
        private float debugRayDistance;
        private RaycastHit debugHit;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            if (animator != null)
                animator.SetBool("Rifle", true);

            currentState = enemyMode == EnemyMode.CombatOnly ? State.Combat : State.PatrolMove;
            agent.stoppingDistance = patrolStoppingDistance;
            
            if (exclamationMark != null)
                exclamationMark.SetActive(false);

            if (questionMark != null)
                questionMark.SetActive(false);
        }

        private void Start()
        {
            if (enemyMode == EnemyMode.CombatOnly)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                hasLastKnownPosition = player != null;

                if (player != null)
                    lastKnownPlayerPosition = player.position;
            }
            else
            {
                SetNextPatrolDestination();
            }
        }

        private void Update()
        {
            if (PauseMenu.IsPaused)
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }

                UpdateAnimator(false, false);
                return;
            }
            if (player == null)
            {
                TryFindPlayer();
                UpdateAnimator(false, false);
                return;
            }

            if (enemyMode == EnemyMode.CombatOnly)
            {
                UpdateCombatOnlyEnemy();
                return;
            }

            bool visibleNow = CanSeePlayer();

            if (visibleNow)
            {
                lastKnownPlayerPosition = player.position;
                hasLastKnownPosition = true;
                lostSightTimer = 0f;

                if (currentState != State.Combat)
                    EnterCombat();
            }
            else if (currentState == State.Combat)
            {
                lostSightTimer += Time.deltaTime;
            }

            switch (currentState)
            {
                case State.PatrolMove:
                    UpdatePatrolMove(visibleNow);
                    break;

                case State.PatrolScan:
                    UpdatePatrolScan(visibleNow);
                    break;

                case State.Combat:
                    UpdateCombat(visibleNow);
                    break;

                case State.Investigate:
                    UpdateInvestigate(visibleNow);
                    break;
            }
        }

        private void UpdateCombatOnlyEnemy()
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;

            bool visibleNow = CanSeePlayer();

            RotateTowardLastKnownPosition();

            bool projectileStillBlocking = activeProjectile != null &&
                                           Time.time - activeProjectileSpawnTime < maxProjectileBlockTime;

            if (visibleNow)
            {
                if (Time.time >= nextFireTime && !projectileStillBlocking)
                {
                    Fire();
                    nextFireTime = Time.time + fireCooldown;
                }

                UpdateAnimator(false, true);
            }
            else
            {
                UpdateAnimator(false, false);
            }
        }

        private void ShowExclamation()
        {
            if (exclamationMark == null)
                return;

            if (exclamationRoutine != null)
                StopCoroutine(exclamationRoutine);

            exclamationRoutine = StartCoroutine(ShowIndicatorTemporarily(exclamationMark));
        }

        private void ShowQuestion()
        {
            if (questionMark == null)
                return;

            if (questionRoutine != null)
                StopCoroutine(questionRoutine);

            questionRoutine = StartCoroutine(ShowIndicatorTemporarily(questionMark));
        }

        private System.Collections.IEnumerator ShowIndicatorTemporarily(GameObject indicator)
        {
            indicator.SetActive(true);
            yield return new WaitForSeconds(indicatorDuration);
            indicator.SetActive(false);
        }
        
        private void TryFindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        private void EnterCombat()
        {
            currentState = State.Combat;
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.stoppingDistance = patrolStoppingDistance;

            ShowExclamation();
            
            if (showDebugLogs)
                Debug.Log($"{name} -> COMBAT");
        }

        private void EnterInvestigate()
        {
            if (enemyMode == EnemyMode.CombatOnly)
                return;

            currentState = State.Investigate;
            agent.isStopped = false;
            agent.stoppingDistance = investigateStoppingDistance;
            investigateTimer = 0f;
            
            if (hasLastKnownPosition)
                agent.SetDestination(lastKnownPlayerPosition);

            ShowQuestion();
            
            if (showDebugLogs)
                Debug.Log($"{name} -> INVESTIGATE");
        }

        private void EnterPatrolScan()
        {
            if (enemyMode == EnemyMode.CombatOnly)
                return;

            currentState = State.PatrolScan;
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            scanCenterRotation = transform.rotation;
            scanLeftRotation = scanCenterRotation * Quaternion.Euler(0f, -scanTurnAngle, 0f);
            scanRightRotation = scanCenterRotation * Quaternion.Euler(0f, scanTurnAngle, 0f);
            scanPhase = 0;
            scanPauseTimer = 0f;

            if (showDebugLogs)
                Debug.Log($"{name} -> SCAN");
        }

        private void EnterPatrolMove()
        {
            if (enemyMode == EnemyMode.CombatOnly)
                return;

            currentState = State.PatrolMove;
            agent.isStopped = false;
            agent.stoppingDistance = patrolStoppingDistance;
            SetNextPatrolDestination();

            if (showDebugLogs)
                Debug.Log($"{name} -> PATROL");
        }

        private void UpdatePatrolMove(bool visibleNow)
        {
            if (visibleNow)
            {
                EnterCombat();
                return;
            }

            bool reached = !agent.pathPending &&
                           agent.remainingDistance <= agent.stoppingDistance &&
                           (!agent.hasPath || agent.velocity.sqrMagnitude < 0.05f);

            if (reached)
            {
                EnterPatrolScan();
                return;
            }

            UpdateAnimator(agent.velocity.sqrMagnitude > 0.05f, false);
        }

        private void UpdatePatrolScan(bool visibleNow)
        {
            if (visibleNow)
            {
                EnterCombat();
                return;
            }

            Quaternion targetRotation = transform.rotation;

            switch (scanPhase)
            {
                case 0:
                    targetRotation = scanLeftRotation;
                    if (RotateTowards(targetRotation, scanTurnSpeed))
                        scanPhase = 1;
                    break;

                case 1:
                    scanPauseTimer += Time.deltaTime;
                    if (scanPauseTimer >= scanPauseTime)
                    {
                        scanPauseTimer = 0f;
                        scanPhase = 2;
                    }
                    break;

                case 2:
                    targetRotation = scanRightRotation;
                    if (RotateTowards(targetRotation, scanTurnSpeed))
                        scanPhase = 3;
                    break;

                case 3:
                    scanPauseTimer += Time.deltaTime;
                    if (scanPauseTimer >= scanPauseTime)
                    {
                        scanPauseTimer = 0f;
                        scanPhase = 4;
                    }
                    break;

                case 4:
                    targetRotation = scanCenterRotation;
                    if (RotateTowards(targetRotation, scanTurnSpeed))
                        EnterPatrolMove();
                    break;
            }

            UpdateAnimator(false, false);
        }

        private void UpdateCombat(bool visibleNow)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            RotateTowardLastKnownPosition();

            bool projectileStillBlocking = activeProjectile != null && Time.time - activeProjectileSpawnTime < maxProjectileBlockTime;

            if (visibleNow)
            {
                lostSightTimer = 0f;

                if (Time.time >= nextFireTime && !projectileStillBlocking)
                {
                    Fire();
                    nextFireTime = Time.time + fireCooldown;
                }
            }
            else if (lostSightTimer >= investigateDelay)
            {
                EnterInvestigate();
                return;
            }

            UpdateAnimator(false, true);
        }

        private void UpdateInvestigate(bool visibleNow)
        {
            if (visibleNow)
            {
                EnterCombat();
                return;
            }

            bool reached = !agent.pathPending &&
                           agent.remainingDistance <= agent.stoppingDistance &&
                           (!agent.hasPath || agent.velocity.sqrMagnitude < 0.05f);

            if (reached)
            {
                investigateTimer += Time.deltaTime;
                if (investigateTimer >= investigateWaitTime)
                {
                    if (hasLastKnownPosition)
                    {
                        hasLastKnownPosition = false;
                    }

                    EnterPatrolMove();
                    return;
                }
            }

            UpdateAnimator(agent.velocity.sqrMagnitude > 0.05f, false);
        }

        private void SetNextPatrolDestination()
        {
            if (enemyMode == EnemyMode.CombatOnly)
                return;

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                if (patrolIndex >= patrolPoints.Length)
                    patrolIndex = 0;

                Transform point = patrolPoints[patrolIndex];
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

                if (point != null)
                    agent.SetDestination(point.position);

                return;
            }

            if (useRandomPatrolIfNoPoints && TryGetRandomNavMeshPoint(transform.position, randomPatrolRadius, out Vector3 result))
            {
                agent.SetDestination(result);
            }
        }

        private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 random2D = Random.insideUnitCircle * radius;
                Vector3 randomPos = origin + new Vector3(random2D.x, 0f, random2D.y);

                if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = origin;
            return false;
        }

        private bool CanSeePlayer()
        {
            debugVisible = false;
            debugInRange = false;
            debugInFOV = false;
            debugRayHitSomething = false;
            debugHitName = "";

            if (player == null)
                return false;

            debugEyePos = eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * eyeHeightOffset;

            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider>();

            debugTargetPos = playerCollider != null
                ? playerCollider.bounds.center
                : player.position + Vector3.up * targetHeightOffset;

            Vector3 toTarget = debugTargetPos - debugEyePos;
            debugRayDistance = toTarget.magnitude;

            if (debugRayDistance > detectionRange)
                return false;

            debugInRange = true;

            if (debugRayDistance <= 0.001f)
                return false;

            debugRayDir = toTarget.normalized;

            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;

            if (flatForward.sqrMagnitude <= 0.001f)
                return false;

            flatForward.Normalize();

            Vector3 flatToTarget = debugTargetPos - debugEyePos;
            flatToTarget.y = 0f;

            if (flatToTarget.sqrMagnitude <= 0.001f)
                return true;

            flatToTarget.Normalize();

            float angle = Vector3.Angle(flatForward, flatToTarget);
            if (angle > fieldOfViewAngle * 0.5f)
                return false;

            debugInFOV = true;

            Debug.DrawRay(debugEyePos, debugRayDir * debugRayDistance, Color.yellow);

            if (Physics.Raycast(
                    debugEyePos,
                    debugRayDir,
                    out debugHit,
                    debugRayDistance + 0.1f,
                    visionMask,
                    QueryTriggerInteraction.Ignore))
            {
                debugRayHitSomething = true;
                debugHitName = debugHit.collider.name + " | Tag: " + debugHit.collider.tag;

                bool hitPlayer = debugHit.collider.CompareTag("Player");

                Debug.DrawRay(
                    debugEyePos,
                    debugRayDir * debugHit.distance,
                    hitPlayer ? Color.green : Color.red
                );

                debugVisible = hitPlayer;
                return hitPlayer;
            }

            debugHitName = "Nothing";
            debugVisible = false;
            return false;
        }

        private void RotateTowardLastKnownPosition()
        {
            if (!hasLastKnownPosition)
                return;

            Vector3 targetPos = lastKnownPlayerPosition + Vector3.up * targetHeightOffset;
            Vector3 dir = targetPos - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, aimTurnSpeed * Time.deltaTime);
        }

        private bool RotateTowards(Quaternion targetRotation, float speed)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            return Quaternion.Angle(transform.rotation, targetRotation) < 1f;
        }

        private bool IsMuzzleAlignedToPlayer()
        {
            if (player == null)
                return false;

            Transform source = muzzle != null ? muzzle : projectileSpawnPoint;
            if (source == null)
                return true;

            Vector3 targetPos = player.position + Vector3.up * targetHeightOffset;
            Vector3 toTarget = (targetPos - source.position).normalized;
            float angle = Vector3.Angle(source.forward, toTarget);

            return angle <= muzzleAimTolerance && debugVisible;
        }

        private void Fire()
        {
            if (projectilePrefab == null || projectileSpawnPoint == null || player == null)
                return;

            if (muzzleFlashPrefab != null && muzzle != null)
            {
                ParticleSystem flash = Instantiate(
                    muzzleFlashPrefab,
                    muzzle.position,
                    muzzle.rotation,
                    muzzle
                );

                flash.Play();
                Destroy(flash.gameObject, 1f);
            }

            Vector3 targetPos = player.position + Vector3.up * targetHeightOffset;
            Vector3 dir = (targetPos - projectileSpawnPoint.position).normalized;

            activeProjectile = Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.LookRotation(dir)
            );

            Rigidbody rb = activeProjectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = dir * projectileSpeed;
#else
                rb.velocity = dir * projectileSpeed;
#endif
            }

            activeProjectileSpawnTime = Time.time;

            ProjectileWatcher watcher = activeProjectile.GetComponent<ProjectileWatcher>();
            if (watcher == null)
                watcher = activeProjectile.AddComponent<ProjectileWatcher>();

            watcher.Init(this);
        }

        public void NotifyProjectileDestroyed(GameObject projectile)
        {
            if (activeProjectile == projectile)
                activeProjectile = null;
        }

        private void UpdateAnimator(bool isWalking, bool isAiming)
        {
            if (animator == null)
                return;

            animator.SetBool("Walk", isWalking);
            animator.SetBool("NoMovement", !isWalking);
            animator.SetBool("Aim", isAiming);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 eyePos = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * eyeHeightOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Vector3 left = Quaternion.AngleAxis(-fieldOfViewAngle * 0.5f, Vector3.up) * transform.forward;
            Vector3 right = Quaternion.AngleAxis(fieldOfViewAngle * 0.5f, Vector3.up) * transform.forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(eyePos, eyePos + left * detectionRange);
            Gizmos.DrawLine(eyePos, eyePos + right * detectionRange);
            Gizmos.DrawLine(eyePos, eyePos + transform.forward * detectionRange);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(eyePos, 0.08f);

            if (player != null)
            {
                Vector3 targetPos = player.position + Vector3.up * targetHeightOffset;
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(targetPos, 0.08f);
            }

            if (debugInRange && debugInFOV)
            {
                Gizmos.color = debugVisible ? Color.green : Color.red;
                Gizmos.DrawLine(debugEyePos, debugEyePos + debugRayDir * debugRayDistance);

                if (debugRayHitSomething)
                {
                    Gizmos.DrawSphere(debugHit.point, 0.12f);
                }
            }

            if (hasLastKnownPosition)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(lastKnownPlayerPosition + Vector3.up * 0.2f, 0.15f);
            }

            if (agent != null && agent.hasPath)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f);
                Vector3 prev = transform.position;
                foreach (Vector3 corner in agent.path.corners)
                {
                    Gizmos.DrawLine(prev, corner);
                    Gizmos.DrawSphere(corner, 0.08f);
                    prev = corner;
                }
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!Application.isPlaying)
                return;

            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.2f) : Vector3.zero;
            if (screenPos.z < 0f)
                return;

            GUI.color = Color.white;
            GUI.Label(
                new Rect(screenPos.x - 70f, Screen.height - screenPos.y - 20f, 220f, 100f),
                $"State: {currentState}\nVisible: {debugVisible}\nInRange: {debugInRange}\nInFOV: {debugInFOV}\nHit: {debugHitName}"
            );
        }
#endif
    }

    public class ProjectileWatcher : MonoBehaviour
    {
        private EnemyAI owner;

        public void Init(EnemyAI enemy)
        {
            owner = enemy;
        }

        private void OnDestroy()
        {
            if (owner != null)
                owner.NotifyProjectileDestroyed(gameObject);
        }
    }
}