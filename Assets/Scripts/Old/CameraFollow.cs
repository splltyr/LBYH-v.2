using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2f;
    public float yOffset = 1f;
    public Transform target;

    [Header("Boundary Settings")]
    public bool useBoundaries = true;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void Update()
    {
        // 1. Calculate the desired position based on target
        float targetX = target.position.x;
        float targetY = target.position.y + yOffset;

        // 2. If boundaries are on, "Clamp" the values so they can't go past the edges
        if (useBoundaries)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
            targetY = Mathf.Clamp(targetY, minY, maxY);
        }

        Vector3 newPos = new Vector3(targetX, targetY, -10f);

        // 3. Smoothly move to that (clamped) position
        transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
    }
}