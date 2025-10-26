using UnityEngine;

public class KnobToRotation : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private bool invertRotation = false;

    private float accumulatedAngle = 0f;

    public void OnKnobValueChanged(float knobAngle)
    {
        if (targetObject == null) return;

        float delta = knobAngle * rotationSpeed;
        if (invertRotation) delta = -delta;

        accumulatedAngle += delta;
        // Debug.Log($"Угол башни: {accumulatedAngle}");

        targetObject.localRotation = Quaternion.AngleAxis(accumulatedAngle, rotationAxis);
    }
}