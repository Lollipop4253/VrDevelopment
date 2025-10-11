using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour
{
    [Header("References")]
    public AmmoPlatform ammoPlatform;
    public Transform shootPoint;
    public float shootForce = 50f;

    [Header("Ammo Prefab")]
    public GameObject ammoPrefab;

    void Start()
    {
        if (shootPoint == null)
        {
            Debug.LogWarning("ShootPoint not assigned. Using Shooter's transform.");
            shootPoint = transform;
        }

        if (ammoPrefab == null)
        {
            Debug.LogError("Ammo Prefab is not assigned!");
        }
    }

    public void TryShoot()
    {
        Debug.Log("Попытка выстрела...");

        if (!ammoPlatform.HasAmmo())
        {
            Debug.Log("❌ Нет снаряда на платформе (не заряжено).");
            return;
        }

        if (ammoPrefab == null)
        {
            Debug.LogError("Ammo Prefab не назначен!");
            return;
        }

        Debug.Log("💥 Создаём снаряд из префаба!");
        GameObject newAmmo = Instantiate(ammoPrefab, shootPoint.position, shootPoint.rotation);
        newAmmo.transform.position += shootPoint.forward * 0.1f;

        ShootAmmo(newAmmo);

        // Удаляем сферу-триггер после выстрела
        RemoveTriggerSphere();
    }

    private void ShootAmmo(GameObject ammo)
    {
        Debug.Log($"💥 Выстрел! Снаряд: {ammo.name}");

        Rigidbody rb = ammo.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("❌ У снаряда нет Rigidbody!");
            return;
        }

        rb.isKinematic = false;
        rb.linearVelocity = shootPoint.forward * shootForce;

        Debug.Log($"🚀 Скорость снаряда: {rb.linearVelocity}");
        Debug.DrawRay(shootPoint.position, shootPoint.forward * 5f, Color.red, 2f); 
    }

    private void RemoveTriggerSphere()
    {
        if (ammoPlatform == null) return;

        GameObject triggerSphere = ammoPlatform.GetCurrentTriggerSphere();

        if (triggerSphere != null)
        {
            Debug.Log($"💥 Удаляем триггер-сферу: {triggerSphere.name}");
            Destroy(triggerSphere);

            // Сбрасываем состояние платформы
            ammoPlatform.ResetLoadedState();
        }
        else
        {
            Debug.LogWarning("⚠️ Нет активной сферы-триггера для удаления.");
        }
    }
}
