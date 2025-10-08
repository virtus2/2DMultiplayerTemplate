using System.Collections;
using UnityEngine;

public class Shake : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform targetTransform;

    [Header("Variables")]
    [SerializeField] private AnimationCurve animationCurve;
    private Vector3 originalPosition;
    private Coroutine coroutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void StartShake(float duration, float strength)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(ShakeCoroutine(duration, strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float remainingTime = duration;

        while (remainingTime > 0f)
        {
            float magnitude = animationCurve.Evaluate((duration - remainingTime) / duration);
            targetTransform.localPosition = originalPosition + Random.insideUnitSphere * magnitude * strength;

            remainingTime -= Time.deltaTime;

            yield return null;
        }

        targetTransform.localPosition = originalPosition;
        coroutine = null;
    }
}