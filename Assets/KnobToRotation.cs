using UnityEngine;

public class KnobToRotation : MonoBehaviour
{
    [SerializeField] private Transform targetObject;        // Объект, который будет вращаться
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Ось вращения (Y по умолчанию)
    [SerializeField] private float maxTargetAngle = 360f;   // На сколько градусов повернётся целевой объект
    [SerializeField] private bool invertRotation = false;   // Инвертировать направление
    [SerializeField] private float RotationSpeed = 0.5f;

    public void OnKnobValueChanged(float knobValue)
    {
        if (targetObject == null) return;

        float currentAngel = targetObject.transform.eulerAngles.z;

        float angle = currentAngel + knobValue * RotationSpeed;
        if (invertRotation) angle = -angle;

        targetObject.localRotation = Quaternion.Euler(rotationAxis * angle);
    }
}