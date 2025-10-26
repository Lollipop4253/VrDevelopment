using UnityEngine;

public class Tank : MonoBehaviour
{
    [Header("Lever References")]
    public LeverController leftLever;
    public LeverController rightLever;
    public CustomTransmissionLever speedLever;

    [Header("Movement Settings")]
    public float moveSpeed = 50f;
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on tank!");
        }
    }

    void FixedUpdate()
    {
        if (leftLever == null || rightLever == null || speedLever == null)
        {
            Debug.LogError("Lever references are not assigned!");
            return;
        }

        float leftValue = leftLever.GetLeverValue();
        float rightValue = rightLever.GetLeverValue();
        float speedValue = speedLever.ChangeTargetPosition(); // 👈 Теперь float от -1 до 1

        // Применяем мёртвую зону
        if (Mathf.Abs(leftValue) < deadZone) leftValue = 0f;
        if (Mathf.Abs(rightValue) < deadZone) rightValue = 0f;

        Debug.Log($"Left Value: {leftValue:F2} | Right Value: {rightValue:F2} | Speed Lever: {speedValue:F2}");

        // 🔴 ОСНОВНАЯ ЛОГИКА: если скорость == 0 — останавливаем
        if (Mathf.Abs(speedValue) < 0.05f) // небольшой порог для центра
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // 💡 Расчёт движения
        float forwardSpeed = (leftValue + rightValue) * 0.5f * moveSpeed;

        // Добавляем базовую скорость вперёд, если рычаги в мёртвой зоне
        if (Mathf.Abs(leftValue) < deadZone && Mathf.Abs(rightValue) < deadZone)
        {
            forwardSpeed = idleForwardSpeed * moveSpeed;
        }

        // 🔄 НЕЛИНЕЙНАЯ ЧУВСТВИТЕЛЬНОСТЬ ПОВОРОТА
        float turnDiff = rightValue - leftValue;
        float turnSpeedValue = Mathf.Sign(turnDiff) * Mathf.Pow(Mathf.Abs(turnDiff), 1.5f) * turnSpeed;

        // Применяем ускорение вперёд/назад от третьего рычага
        forwardSpeed *= speedValue; // 👈 Умножаем на -1..1

        Debug.Log($"Forward Force: {forwardSpeed:F2} | Turn Torque: {turnSpeedValue:F2}");

        rb.AddForce(-transform.right * forwardSpeed, ForceMode.Force);
        rb.AddTorque(Vector3.up * turnSpeedValue, ForceMode.Force);

        // Ограничения
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        if (rb.angularVelocity.magnitude > maxAngularSpeed)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Замедление при нейтральных рычагах
        if (Mathf.Abs(leftValue) < deadZone && Mathf.Abs(rightValue) < deadZone)
        {
            rb.angularVelocity *= angularDampingFactor;
        }
    }
}