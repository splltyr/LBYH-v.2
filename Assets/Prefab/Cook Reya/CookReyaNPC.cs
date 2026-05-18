using UnityEngine;
using System.Collections;
using TMPro;

public class CookReyaNPC : MonoBehaviour
{
    [Header("Detection")]
    public float interactionRange = 5f; 
    public LayerMask playerLayer;

    private bool prevGamepadInteract = false;

    void Update()
    {
        bool isPlayerNearby = Physics2D.OverlapCircle(transform.position, interactionRange, playerLayer);
        
        bool interacted = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) interacted = true;

        foreach (var gamepad in UnityEngine.InputSystem.Gamepad.all)
        {
            bool currentInteract = gamepad.buttonSouth.isPressed || gamepad.buttonWest.isPressed;
            if (currentInteract && !prevGamepadInteract) interacted = true;
            prevGamepadInteract = currentInteract;
            if (currentInteract) break; // found an active button
        }

        if (isPlayerNearby && interacted)
        {
            if (Scene3Manager.Instance != null)
            {
                Scene3Manager.Instance.InteractWithReya();
            }
            else
            {
                Debug.LogError("Scene3Manager is missing in the scene!");
            }
        }
    }
}