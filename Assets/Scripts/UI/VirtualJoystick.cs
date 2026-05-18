using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    [Tooltip("The background circle of the joystick.")]
    public RectTransform background;
    [Tooltip("The handle/knob of the joystick.")]
    public RectTransform handle;

    private Vector2 inputVector;

    private void Start()
    {
        if (background == null) background = GetComponent<RectTransform>();
        if (handle == null && transform.childCount > 0) handle = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            // Normalize the position based on the background's size
            pos.x = (pos.x / background.sizeDelta.x);
            pos.y = (pos.y / background.sizeDelta.y);

            // Pivot compensation (assuming pivot is 0.5, 0.5)
            inputVector = new Vector2(pos.x * 2 - 1, pos.y * 2 - 1);
            
            // Keep it within the circle
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Move the handle visually
            if (handle != null)
            {
                handle.anchoredPosition = new Vector2(
                    inputVector.x * (background.sizeDelta.x / 2),
                    inputVector.y * (background.sizeDelta.y / 2));
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
    }

    // Expose these methods to your Player Script
    public float Horizontal() { return inputVector.x; }
    public float Vertical() { return inputVector.y; }
}
