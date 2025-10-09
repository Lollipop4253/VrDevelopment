using UnityEngine;

public class AmmoPlatform : MonoBehaviour
{
    public bool isLoaded = false;
    private GameObject currentTriggerSphere = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AmmoTrigger"))
        {
            isLoaded = true;
            currentTriggerSphere = other.gameObject;
            Debug.Log($"✅ Платформа заряжена. Триггер: {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AmmoTrigger"))
        {
            isLoaded = false;
            currentTriggerSphere = null;
            Debug.Log($"➖ Платформа разряжена. Ушёл триггер: {other.name}");
        }
    }

    public bool HasAmmo()
    {
        return isLoaded;
    }

    public GameObject GetCurrentTriggerSphere()
    {
        return currentTriggerSphere;
    }

    public void ResetLoadedState()
    {
        isLoaded = false;
        currentTriggerSphere = null;
        Debug.Log("🔄 Состояние платформы сброшено вручную.");
    }
}