using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class Lever3PosController : MonoBehaviour
{
    [Header("References")]
    public XRLever lever;

    [Header("Settings")]
    public float value = 0f;
    [Tooltip("Мёртвая зона в центре — не влияет на края")]
    public float centerDeadzone = 5f;
    public float smoothing = 10f;

    private Transform handle;
    private float targetValue;

    void Start()
    {
        if (lever == null)
        {
            Debug.LogError("XRLever reference is not assigned!", this);
            return;
        }

        handle = lever.transform;
        if (handle == null)
        {
            Debug.LogError("Handle transform is not assigned in XRLever!", this);
            return;
        }
    }

    void Update()
    {
        if (lever == null || handle == null) return;

        float angle = handle.localEulerAngles.x;
        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) < centerDeadzone)
        {
            angle = 0f;
        }

        if (lever.isSelected)
        {
            targetValue = angle;
        }
        else
        {
            if (value < -0.3f * lever.maxAngle)
            {
                targetValue = lever.minAngle;
            }
            else if (value > 0.3f * lever.maxAngle)
            {
                targetValue = lever.maxAngle;
            }
            else
            {
                targetValue = 0;
                handle.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }

        // Плавное изменение
        value = Mathf.Lerp(value, targetValue, Time.deltaTime * smoothing);

    }

    public float GetLeverValue()
    {
        return value;
    }
}