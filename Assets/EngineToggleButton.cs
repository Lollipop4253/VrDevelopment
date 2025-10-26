using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Content.Interaction;

public class EngineToggleButton : MonoBehaviour
{
    [SerializeField] private Tank tank; // Ссылка на твой танк

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        tank?.ToggleEngine();
    }
}