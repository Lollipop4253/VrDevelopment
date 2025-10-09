using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorAngleTracker : MonoBehaviour
{
    public HingeJoint hingeJoint;
    private float totalAngle = 0f;
    private float lastAngle = 0f;

    [Serializable]
    public class ValueChangeEvent : UnityEvent<float> { }

    [SerializeField]
    [Tooltip("Events to trigger when the knob is rotated")]
    private ValueChangeEvent m_OnValueChange = new ValueChangeEvent();
    public ValueChangeEvent onValueChange => m_OnValueChange;

    [SerializeField]
    private bool useLimits = false;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float minAngle;

    void Start()
    {
        if (hingeJoint == null)
            hingeJoint = GetComponent<HingeJoint>();

        if (hingeJoint != null)
        {
            lastAngle = GetHingeAngle();
            totalAngle = 0f;
        }
    }

    void Update()
    {
        if (hingeJoint == null) return;
        
        JointLimits limits = hingeJoint.limits;

        if (useLimits)
        {
            if (totalAngle >= maxAngle - 100)
            {
                hingeJoint.extendedLimits = true;
                hingeJoint.useLimits = true;

                limits.max = maxAngle;
                limits.min = 0;
                hingeJoint.limits = limits;
            }
            else if (totalAngle <= minAngle + 100)
            {
                hingeJoint.extendedLimits = true;
                hingeJoint.useLimits = true;
                
                limits.max = 0;
                limits.min = -1 * (360 - (totalAngle + 100) % 360);
                hingeJoint.limits = limits; 
            }
            else
            {
                hingeJoint.useLimits = false;
                hingeJoint.extendedLimits = false;

                limits.max = 0;
                limits.min = 0;
                hingeJoint.limits = limits;
            }
        }

        float currentAngle = GetHingeAngle();

        float delta = currentAngle - lastAngle;

        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;

        totalAngle += delta;
        lastAngle = currentAngle;

        m_OnValueChange.Invoke(delta);

        Debug.Log($"Общий угол поворота: {totalAngle:F2}°");
    }

    float GetHingeAngle()
    {
        Vector3 axis = hingeJoint.axis.normalized;
        Vector3 forward = transform.forward;

        Vector3 projectedForward = Vector3.ProjectOnPlane(forward, axis).normalized;
        Vector3 referenceForward = Vector3.ProjectOnPlane(transform.parent.forward, axis).normalized;

        if (projectedForward.magnitude < 0.01f || referenceForward.magnitude < 0.01f)
            return 0f;

        float angle = Vector3.SignedAngle(referenceForward, projectedForward, axis);

        if (angle < 0f) angle += 360f;

        return angle;
    }
}