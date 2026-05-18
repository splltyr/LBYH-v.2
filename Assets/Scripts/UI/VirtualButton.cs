using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPressed;
    private bool wasPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        wasPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    // Returns true if pressed, and consumes the press so it doesn't trigger multiple times
    public bool ConsumePress()
    {
        if (wasPressed)
        {
            wasPressed = false;
            return true;
        }
        return false;
    }

    public bool IsPressed()
    {
        return isPressed;
    }
}
