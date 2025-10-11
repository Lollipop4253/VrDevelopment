using UnityEngine;

public class KnobToRotation : MonoBehaviour
{
    [SerializeField] private Transform targetObject;        // ������, ������� ����� ���������
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // ��� �������� (Y �� ���������)
    [SerializeField] private float maxTargetAngle = 360f;   // �� ������� �������� ���������� ������� ������
    [SerializeField] private bool invertRotation = false;   // ������������� �����������
    [SerializeField] private float RotationSpeed = 0.5f;

    public void OnKnobValueChanged(float knobValue)
    {
        if (targetObject == null) return;

        Debug.Log(knobValue);

        float currentAngel = targetObject.transform.eulerAngles.z;

        float angle = currentAngel + knobValue * RotationSpeed;
        if (invertRotation) angle = -angle;

        targetObject.localRotation = Quaternion.Euler(rotationAxis * angle);
    }
}