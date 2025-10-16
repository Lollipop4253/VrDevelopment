using UnityEngine;

public class LeverController : MonoBehaviour
{
    [Header("Lever Settings")]
    public float maxAngle = 40f;

    private Vector3 initialForward;

    void Start()
    {
        initialForward = transform.forward;
    }

    public float GetLeverValue()
    {
        Vector3 currentForward = transform.forward;
        float angle = Vector3.SignedAngle(initialForward, currentForward, Vector3.up);

        if (angle > 180f) angle -= 360f;
        angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        float normalizedAngle = Mathf.Abs(angle / maxAngle);

        // Для отладки — можно раскомментировать
         //Debug.Log($"Raw Angle: {angle:F2}, Normalized: {normalizedAngle}");

        return normalizedAngle;
    }
}