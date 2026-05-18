using UnityEngine;

public class ObjectivePointer : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;        // The Library Puzzle
    public float rotationSpeed = 10f;
    public Vector3 offset = new Vector3(0, 2.5f, 0); // Height above player

    private Transform player;

    void Start()
    {
        // Auto-find the player
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (target == null || player == null) return;

        // 1. Follow the player
        transform.position = player.position + offset;

        // 2. Point towards the target (2D Rotation)
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Apply rotation
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
