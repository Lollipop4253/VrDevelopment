using UnityEngine;
using UnityEngine.InputSystem;

public class SceneChangerVR : MonoBehaviour
{
    [SerializeField] private Transform targetPosition;

    public void OnActivate()
    {
        // Находим объект персонажа по имени
        GameObject playerSetup = GameObject.Find("XR Interaction Setup (MP Variant)");

        if (playerSetup != null)
        {
            Transform child = playerSetup.transform.Find("Locomotion");
            child.gameObject.SetActive(false);
            playerSetup.transform.position = targetPosition.position;
        }
    }
}