using UnityEngine;

public class Tank : MonoBehaviour
{
    [Header("Lever References")]
    public LeverController leftLever;
    public LeverController rightLever;
    public CustomTransmissionLever speedLever;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 50f;
    public float turnSpeed = 30f;
    public float deadZone = 0.1f;

    [Header("Speed Limits")]
    public float maxSpeed = 10f;
    public float maxAngularSpeed = 3f;

    [Header("Damping")]
    public float angularDampingFactor = 0.8f;

    [Header("Idle Forward Speed")]
    public float idleForwardSpeed = 0.3f;

    private Rigidbody rb;
    private bool engineRunning = false;

    // Состояния повреждений
    private bool leftTrackDisabled = false;
    private bool rightTrackDisabled = false;
    private float globalSpeedMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on tank!");
        }
    }

    public void ToggleEngine()
    {
        engineRunning = !engineRunning;
        if (!engineRunning)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (leftLever == null || rightLever == null || speedLever == null)
        {
            Debug.LogError("Lever references are not assigned!");
            return;
        }

        // Получаем "сырые" значения (уже с учётом deadZone и disabled)
        float leftInput = leftTrackDisabled ? 0f : (Mathf.Abs(leftLever.GetLeverValue()) < deadZone ? 0f : leftLever.GetLeverValue());
        float rightInput = rightTrackDisabled ? 0f : (Mathf.Abs(rightLever.GetLeverValue()) < deadZone ? 0f : rightLever.GetLeverValue());

        float speedValue = engineRunning ? speedLever.ChangeTargetPosition() : 0f;

        if (Mathf.Abs(speedValue) < 0.05f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Силы от гусениц
        float leftForce = leftInput * baseMoveSpeed * globalSpeedMultiplier;
        float rightForce = rightInput * baseMoveSpeed * globalSpeedMultiplier;

        // Поступательное движение
        float forwardSpeed = (leftForce + rightForce) * 0.5f * speedValue;

        // Поворот — напрямую от разницы сил
        float turnSpeedValue = (rightForce - leftForce) * turnSpeed * 0.05f; // коэффициент подбери

        // Отключаем idle, если гусеницы повреждены
        if (Mathf.Abs(leftInput) < 0.01f && Mathf.Abs(rightInput) < 0.01f)
        {
            if (!leftTrackDisabled && !rightTrackDisabled)
            {
                forwardSpeed = idleForwardSpeed * baseMoveSpeed * globalSpeedMultiplier * speedValue;
            }
            // Иначе — не даём idle
        }

        // Применяем
        rb.AddForce(-transform.right * forwardSpeed, ForceMode.Force);
        rb.AddTorque(Vector3.up * turnSpeedValue, ForceMode.Force);

        float currentMaxSpeed = maxSpeed * globalSpeedMultiplier;
        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }

        if (rb.angularVelocity.magnitude > maxAngularSpeed)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (Mathf.Abs(leftInput) < deadZone && Mathf.Abs(rightInput) < deadZone)
        {
            rb.angularVelocity *= angularDampingFactor;
        }
    }

    // === ПОВРЕЖДЕНИЯ ЧЕРЕЗ INVOKE ===

    public void ApplyTrackDamage(string trackSide, float duration)
    {
        if (trackSide == "Left")
        {
            leftTrackDisabled = true;
            Invoke(nameof(RestoreLeftTrack), duration);
        }
        else if (trackSide == "Right")
        {
            rightTrackDisabled = true;
            Invoke(nameof(RestoreRightTrack), duration);
        }
        else
        {
            Debug.LogWarning("Invalid track side. Use 'Left' or 'Right'.");
        }
    }

    public void ApplyGlobalSlow(float speedMultiplier, float duration)
    {
        globalSpeedMultiplier = Mathf.Clamp01(speedMultiplier);
        Invoke(nameof(RestoreGlobalSpeed), duration);
    }

    // === ВОССТАНОВЛЕНИЕ ===

    void RestoreLeftTrack()
    {
        leftTrackDisabled = false;
    }

    void RestoreRightTrack()
    {
        rightTrackDisabled = false;
    }

    void RestoreGlobalSpeed()
    {
        globalSpeedMultiplier = 1f;
    }
}