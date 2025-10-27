using UnityEngine;

public class KnobToRotation : MonoBehaviour
{
    [SerializeField] private Transform targetObjectTank;
    [SerializeField] private Transform targetObjectTankInside;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float baseRotationSpeed = 0.5f; // исходная скорость
    [SerializeField] private bool invertRotation = false;

    private float currentRotationSpeed;
    private float accumulatedAngle = 0f;

    void Awake()
    {
        currentRotationSpeed = baseRotationSpeed;
    }

    public void OnKnobValueChanged(float knobAngle)
    {
        if (targetObjectTank == null) return;
        if (targetObjectTankInside == null) return;

        float delta = knobAngle * currentRotationSpeed;
        if (invertRotation) delta = -delta;

        accumulatedAngle += delta;
        targetObjectTank.localRotation = Quaternion.AngleAxis(accumulatedAngle, rotationAxis);
        targetObjectTankInside.localRotation = Quaternion.AngleAxis(accumulatedAngle, rotationAxis);
    }

    public void UpdateSpeedToTime(float reducedSpeed, float duration)
    {
        float originalSpeed = currentRotationSpeed;

        currentRotationSpeed = reducedSpeed;

        Invoke(nameof(RestoreSpeed), duration);
    }

    void RestoreSpeed()
    {
        currentRotationSpeed = baseRotationSpeed;
    }
}