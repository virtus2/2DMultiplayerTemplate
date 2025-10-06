using UnityEngine;
using UnityEngine.Events;

public class SquashAndStretch : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private DamageReceiver damageReceiver;

    [Header("Variables")]
    [SerializeField] private float duration;
    [SerializeField] private float squashX;
    [SerializeField] private float squashY;
    [SerializeField] private AnimationCurve recoveryAnimationCurve;

    private Vector3 squashedSize = Vector3.zero;
    private Vector3 originalSize = Vector3.one;
    private float elapsedTime = 0f;
    private bool isPlaying = false;

    private void Awake()
    {
        originalSize = targetTransform.localScale;
    }

    private void OnEnable()
    {
        damageReceiver.OnDamageTaken += Play;
    }

    private void OnDisable()
    {
        damageReceiver.OnDamageTaken -= Play;
    }

    private void Update()
    {
        if (isPlaying)
        {
            if (elapsedTime < duration)
            {
                float squashedX = targetTransform.localScale.x;
                float squashedY = targetTransform.localScale.y;
                float t = recoveryAnimationCurve.Evaluate(elapsedTime / duration);
                targetTransform.localScale = Vector3.Lerp(squashedSize, originalSize, t);

                elapsedTime += Time.deltaTime;
            }
            else
            {
                elapsedTime = 0f;
                targetTransform.localScale = originalSize;
                isPlaying = false;
            }
        }
    }

    public void Play()
    {
        isPlaying = true;

        elapsedTime = 0f;
        squashedSize = new Vector3(originalSize.x * squashX, originalSize.y * squashY, targetTransform.localScale.z);
        targetTransform.localScale = squashedSize;
    }
}