using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFeedback : MonoBehaviour
{
    [Header("Red Overlay")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float maxAlpha = 0.45f;
    [SerializeField] private float fadeInTime = 0.05f;
    [SerializeField] private float fadeOutTime = 0.2f;

    [Header("Screen Shake")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeMagnitude = 0.12f;

    private Coroutine overlayRoutine;
    private Coroutine shakeRoutine;
    private Vector3 originalCameraLocalPos;

    private void Awake()
    {
        if (cameraTransform != null)
            originalCameraLocalPos = cameraTransform.localPosition;

        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }
    }

    public void PlayHitFeedback()
    {
        if (overlayRoutine != null)
            StopCoroutine(overlayRoutine);
        overlayRoutine = StartCoroutine(FlashOverlay());

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeCamera());
    }

    private IEnumerator FlashOverlay()
    {
        if (damageOverlay == null)
            yield break;

        float t = 0f;
        Color c = damageOverlay.color;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInTime);
            damageOverlay.color = c;
            yield return null;
        }

        c.a = maxAlpha;
        damageOverlay.color = c;

        t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutTime);
            damageOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        damageOverlay.color = c;
    }

    private IEnumerator ShakeCamera()
    {
        if (cameraTransform == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            offset.z = 0f;

            cameraTransform.localPosition = originalCameraLocalPos + offset;
            yield return null;
        }

        cameraTransform.localPosition = originalCameraLocalPos;
    }
}