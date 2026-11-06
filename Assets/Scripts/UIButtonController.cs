using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ClawController clawController;
    public bool isGrabButton = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[UIButtonController] OnPointerDown: {(isGrabButton ? "СОГНУТЬ" : "РАЗОГНУТЬ")}");

        if (clawController == null)
        {
            Debug.LogError("[UIButtonController] ОШИБКА: clawController is null!");
            return;
        }

        if (isGrabButton)
            clawController.StartGrabbing();
        else
            clawController.StartReleasing();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[UIButtonController] OnPointerUp: {(isGrabButton ? "СОГНУТЬ" : "РАЗОГНУТЬ")}");

        if (clawController == null) return;

        if (isGrabButton)
            clawController.StopGrabbing();
        else
            clawController.StopReleasing();
    }
}
