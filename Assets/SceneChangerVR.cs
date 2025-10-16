using UnityEngine;
using UnityEngine.InputSystem;

public class SceneChangerVR : MonoBehaviour
{
    [SerializeField] private Transform targetPosition;

    public void OnActivate()
    {
        GameObject playerSetup = GameObject.Find("XR Interaction Setup (MP Variant)");

        if (playerSetup != null)
        {
            Transform locomotion = playerSetup.transform.Find("Locomotion");
            if (locomotion != null)
            {
                locomotion.gameObject.SetActive(false);
            }

            playerSetup.transform.SetParent(targetPosition);
            playerSetup.transform.localPosition = Vector3.zero;
            playerSetup.transform.localRotation = Quaternion.identity;
        }
    }
}