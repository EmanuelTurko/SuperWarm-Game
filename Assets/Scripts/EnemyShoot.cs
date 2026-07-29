using System.Collections;
using Assets.Scripts;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject TriggerMarker;
    private bool Trigger = false;

    private GameObject activeProjectile;
    private Animator animator;

    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float targetHeightOffset = 1.2f;
    
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfViewAngle = 120f;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Aiming")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private bool aimVertically = false;

    
    [Header("Shooting")]
    [SerializeField] private float muzzleAimTolerance = 8f;
    [SerializeField] private float shootDuration = 0.8f;
    
    [Header("Turning")]
    [SerializeField] private float turnInterval = 1f;
    [SerializeField] private float turnSpeedDegrees = 180f;

    private float turnTimer = 0f;
    private Quaternion cachedTargetRotation;
    private bool hasCachedTurnTarget = false;

    private bool isShooting = false;
    
    [SerializeField] private float fireRate = 1f;
    private float nextShotTime = 0f;

    
    private bool isAiming;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        TriggerMarker.SetActive(false);
    }

    private void Update()
    {
        if (isShooting)
            return;
        
        isAiming = CanDetectPlayer();
        if (CanDetectPlayer() && !Trigger)
        {
            StartCoroutine(ShowTriggerMarker());
        }
        UpdateShootTimer();
        if (animator != null)
            animator.SetBool("Aim", isAiming);
    }
    private IEnumerator ShowTriggerMarker()
    {
        TriggerMarker.SetActive(true);
        Trigger = true;

        yield return new WaitForSeconds(1.5f);

        TriggerMarker.SetActive(false);
    }
    private void LateUpdate()
    {
        if (isShooting)
            return;

        if (!isAiming || player == null || muzzle == null)
            return;

        turnTimer += Time.deltaTime;

        if (!hasCachedTurnTarget || turnTimer >= turnInterval)
        {
            cachedTargetRotation = GetAimRotation();
            hasCachedTurnTarget = true;
            turnTimer = 0f;
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            cachedTargetRotation,
            turnSpeedDegrees * Time.deltaTime
        );
    }
    private Quaternion GetAimRotation()
    {
        Vector3 targetPoint = player.position + Vector3.up * 1.5f;
        Vector3 toPlayer = targetPoint - muzzle.position;

        if (!aimVertically)
            toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f)
            return transform.rotation;

        Quaternion desiredMuzzleRotation = Quaternion.LookRotation(toPlayer.normalized);
        Quaternion animOffset = Quaternion.Inverse(transform.rotation) * muzzle.rotation;
        Quaternion correctedRootRotation = desiredMuzzleRotation * Quaternion.Inverse(animOffset);

        return correctedRootRotation;
    }

    private bool CanDetectPlayer()
    {
        if (player == null)
            return false;

        Vector3 eyeOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPoint = player.position + Vector3.up * 1.5f;

        Vector3 toTarget = targetPoint - eyeOrigin;
        float distance = toTarget.magnitude;

        if (distance > detectionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toTarget.normalized);

        if (angle > fieldOfViewAngle)
            return false;

        if (Physics.Raycast(eyeOrigin, toTarget.normalized, out RaycastHit hit, distance, obstructionMask))
        {
            if (!hit.transform.IsChildOf(player))
                return false;
        }

        return true;
    }

    private void RotateToAimMuzzleAtPlayer()
    {
        Vector3 targetPoint = player.position + Vector3.up * 1.5f;
        Vector3 toPlayer = targetPoint - muzzle.position;

        if (!aimVertically)
            toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f)
            return;

        Quaternion desiredMuzzleRotation = Quaternion.LookRotation(toPlayer.normalized);
        Quaternion animOffset = Quaternion.Inverse(transform.rotation) * muzzle.rotation;
        Quaternion correctedRootRotation = desiredMuzzleRotation * Quaternion.Inverse(animOffset);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            correctedRootRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void UpdateShootTimer()
    {
        if (!isAiming || player == null || muzzle == null)
            return;

        Vector3 targetPoint = player.position + Vector3.up * targetHeightOffset;
        Vector3 toTarget = (targetPoint - muzzle.position).normalized;

        float angleToTarget = Vector3.Angle(muzzle.forward, toTarget);
        bool playerInsideMuzzleRay = angleToTarget <= muzzleAimTolerance;

        if (!playerInsideMuzzleRay)
            return;

        if (Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + fireRate;
        }
    }
    private void Shoot()
    {
        if (activeProjectile != null) return;

        isShooting = true;

        if (animator != null)
            animator.SetBool("Shoot01", true);

        if (enemyProjectilePrefab != null && projectileSpawnPoint != null && player != null)
        {
            Vector3 targetPoint = player.position + Vector3.up * targetHeightOffset;
            Vector3 shootDirection = (targetPoint - projectileSpawnPoint.position).normalized;

            activeProjectile = Instantiate(
                enemyProjectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.LookRotation(shootDirection)
            );

            Rigidbody rb = activeProjectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
                rb.linearVelocity = shootDirection * projectileSpeed;
                rb.WakeUp();
            }
        }
        else
        {
            Debug.LogWarning("Enemy projectile prefab, spawn point, or player is missing.");
        }

        Invoke(nameof(EndShoot), shootDuration);
    }
    private void EndShoot()
    {
        isShooting = false;

        if (animator != null)
            animator.SetBool("Shoot01", false);
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 eyeOrigin = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Vector3 fovLineLeft = Quaternion.AngleAxis(-fieldOfViewAngle * 0.5f, Vector3.up) * transform.forward;
        Vector3 fovLineRight = Quaternion.AngleAxis(fieldOfViewAngle * 0.5f, Vector3.up) * transform.forward;
        Gizmos.DrawLine(eyeOrigin, eyeOrigin + fovLineLeft * detectionRange);
        Gizmos.DrawLine(eyeOrigin, eyeOrigin + fovLineRight * detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(eyeOrigin, eyeOrigin + transform.forward * 2f);

        if (player != null)
        {
            Vector3 targetPoint = player.position + Vector3.up * 1.5f;

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(targetPoint, 0.1f);
            Gizmos.DrawLine(eyeOrigin, targetPoint);
        }

        if (muzzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(muzzle.position, muzzle.position + muzzle.forward * 3f);
        }
    }
}