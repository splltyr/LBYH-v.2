using UnityEngine;

public class TalaFollow : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerTransform; 
    public Vector3 offset = new Vector3(-1.5f, 1.5f, 0f); 

    [Header("Movement Settings")]
    public float smoothTime = 0.3f;
    
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. Calculate Target Position
        // We check Yves' scale to see which way he faces
        float direction = playerTransform.localScale.x > 0 ? 1 : -1;
        
        // Offset flips so she stays behind him
        Vector3 targetPosition = playerTransform.position + new Vector3(offset.x * direction, offset.y, offset.z);

        // 2. Smooth Movement
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );

        // 3. FIX THE STRETCH: Set absolute scale values
        // This prevents her from inheriting any weird scaling from parents
        transform.localScale = new Vector3(direction, 1, 1);
    }
}