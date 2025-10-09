using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomTransmissionLever : MonoBehaviour
{
    [SerializeField] private HingeJoint hingeJoint;
    [SerializeField] private GameObject visualLever;

    public float ChangeTargetPosition()
    {
        float xAngle = visualLever.transform.eulerAngles.x;
        JointSpring spring = hingeJoint.spring;

        if (xAngle < 341f)
        {
            spring.spring = 100f;
            spring.damper = 15f;
            spring.targetPosition = -10f;
            hingeJoint.spring = spring;
            hingeJoint.useSpring = true;
            return -1f;
        }
        else if (xAngle > 347f)
        {
            spring.spring = 100f;
            spring.damper = 15f;
            spring.targetPosition = 10f;
            hingeJoint.spring = spring;
            hingeJoint.useSpring = true;
            return 1f;
        }
        else
        {
            spring.spring = 100f;
            spring.damper = 15f;
            spring.targetPosition = 0f;
            hingeJoint.spring = spring;
            hingeJoint.useSpring = true;
            return 0f;
        }
    }
}