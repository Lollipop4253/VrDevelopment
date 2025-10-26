using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Ось вращения (Y)
    [SerializeField] private float rotationSpeed = 1.5f;       // Скорость реакции
    [SerializeField] private bool useHapticsOnFullRotation = false; // Отдача при полном обороте

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Quaternion initialLocalRotation;

    private float totalAngle = 0f;           // Полный угол (может быть >360)
    private float previousAngle = 0f;        // Предыдущий угол в диапазоне [-180, 180]
    private int fullRotations = 0;           // Счётчик полных оборотов

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        initialLocalRotation = transform.localRotation;

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is required!");
            return;
        }

        // Блокируем всё, кроме вращения вокруг нужной оси
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!grabInteractable.isSelected) return;

        UpdateRotation();
    }

    void UpdateRotation()
    {
        var interactor = grabInteractable.interactorsSelecting[0];
        Vector3 controllerPos = interactor.transform.position;
        Vector3 objectPos = transform.position;

        // Вектор от объекта к контроллеру
        Vector3 toController = controllerPos - objectPos;

        // Проекция на плоскость, перпендикулярную оси вращения
        Vector3 projected = Vector3.ProjectOnPlane(toController, rotationAxis).normalized;

        if (projected.magnitude < 0.05f) return;

        // Угол в плоскости (всегда от -180 до +180)
        float currentAngle = Vector3.SignedAngle(Vector3.right, projected, rotationAxis);

        // Вычисляем delta с учётом перехода через 0/360
        float delta = currentAngle - previousAngle;

        // Корректируем скачки при переходе через границы
        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;

        totalAngle += delta * rotationSpeed;

        previousAngle = currentAngle;

        // Применяем полный угол (может быть 720, -360 и т.д.)
        transform.localRotation = initialLocalRotation * Quaternion.AngleAxis(totalAngle, rotationAxis);

        // 🎯 Дополнительно: haptics при каждом полном обороте
        if (useHapticsOnFullRotation && interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            int newFullRotations = Mathf.FloorToInt(Mathf.Abs(totalAngle) / 360f);
            if (newFullRotations > fullRotations)
            {
                controllerInteractor.xrController?.SendHapticImpulse(0.3f, 0.1f);
                fullRotations = newFullRotations;
            }
        }
    }
}