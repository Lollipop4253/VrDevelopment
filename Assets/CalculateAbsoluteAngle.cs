using UnityEngine;

public class CalculeteAbsoluteAngle : MonoBehaviour
{
    public HingeJoint hingeJoint;

    [SerializeField] float updateThreshold;
    [SerializeField] private KnobToRotation receiver1;
    [SerializeField] private KnobToRotation receiver2;
    [SerializeField] private bool useLimits = false;
    [SerializeField] private float maxAngle;
    [SerializeField] private float minAngle;

    float lastAngle;
    float currentAngle;
    float globalAngle = 0f;
    float delta;
    bool reverse;

    void Start()
    {
        JointSpring spring = hingeJoint.spring;
        spring.targetPosition = 0f;
        spring.spring = 1000000;
        hingeJoint.spring = spring;
    }
    void Update()
    {
        currentAngle = Mathf.Repeat(hingeJoint.angle, 360f);

        if (Mathf.Abs(currentAngle - lastAngle) >= updateThreshold)
        {
            /// Переход через 0
            if ((lastAngle - 180) < 0 && (currentAngle - 180) > 0)
            {
                if (lastAngle > 170) {; }
                else
                {
                    reverse = true;
                    if (globalAngle > -300 && globalAngle < 300) globalAngle = currentAngle - 360;
                }
            }
            else if ((currentAngle - 180) < 0 && (lastAngle - 180) > 0)
            {
                if (lastAngle < 190) {; }
                else
                {
                    reverse = false;
                    if (globalAngle < 300 && globalAngle > -300) globalAngle = currentAngle;
                }
            }

            delta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, lastAngle));

            JointLimits limits = hingeJoint.limits;

            if (useLimits)
            {
                if (globalAngle >= maxAngle - 100)
                {
                    hingeJoint.extendedLimits = true;
                    hingeJoint.useLimits = true;

                    limits.max = maxAngle;
                    limits.min = 0;
                    hingeJoint.limits = limits;
                    Debug.Log("1");
                }
                else if (globalAngle <= minAngle + 100)
                {
                    hingeJoint.extendedLimits = true;
                    hingeJoint.useLimits = true;
                    
                    limits.max = 360 + (minAngle % 360);
                    limits.min = minAngle % 360;
                    hingeJoint.limits = limits;
                    Debug.Log("2");
                }
                else
                {
                    hingeJoint.useLimits = false;
                    hingeJoint.extendedLimits = false;

                    limits.max = 0;
                    limits.min = 0;
                    hingeJoint.limits = limits;
                    Debug.Log("3");
                }
            }

            if (reverse)
            {
                if (currentAngle > lastAngle)
                {
                    reverse = false;
                }
                else
                {
                    globalAngle -= delta;
                    receiver1?.OnKnobValueChanged(-delta);
                    receiver2?.OnKnobValueChanged(-delta);
                }
            }
            else if (!reverse)
            {
                if (currentAngle < lastAngle)
                {
                    reverse = true;
                }
                else
                {
                    globalAngle += delta;
                    receiver1?.OnKnobValueChanged(delta);
                    receiver2?.OnKnobValueChanged(delta);
                }
            }
            lastAngle = currentAngle;

            Debug.Log($"{globalAngle} {reverse} {delta}");
        }
    }

    public void enableVelocity()
    {
        JointSpring spring = hingeJoint.spring;
        spring.targetPosition = 0;
        spring.spring = 0;
        hingeJoint.spring = spring;
    }
    
    public void disableVelocity()
    {
        JointSpring spring = hingeJoint.spring;
        spring.targetPosition = hingeJoint.angle;
        spring.spring = 1000000;
        hingeJoint.spring = spring;
    }
}
