using UnityEngine;

public class BossShake : MonoBehaviour
{
    public static BossShake Instance;

    private Vector3 originalPosition;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;
    private bool isShaking = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isShaking)
        {
            shakeDuration -= Time.deltaTime;

            if (shakeDuration > 0)
            {
                // Random shake offset
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
                shakeOffset.z = 0; // Keep Z unchanged for 2D
                transform.localPosition = originalPosition + shakeOffset;
            }
            else
            {
                // Stop shaking
                isShaking = false;
                shakeDuration = 0f;
                transform.localPosition = originalPosition;
            }
        }
    }

    /// <summary>
    /// Shake the camera
    /// </summary>
    /// <param name="duration">How long to shake (seconds)</param>
    /// <param name="intensity">How intense (distance)</param>
    public void Shake(float duration, float intensity)
    {
        originalPosition = transform.localPosition;
        shakeDuration = duration;
        shakeIntensity = intensity;
        isShaking = true;

        Debug.Log($"[CameraShake] Shaking for {duration}s at intensity {intensity}");
    }
}