using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Vector3 localScale;

    void Start() { localScale = transform.localScale; }

    void LateUpdate()
    {
        // Keeps the health bar from flipping when the boss flips
        Vector3 parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(
            localScale.x / Mathf.Sign(parentScale.x), 
            localScale.y, 
            localScale.z
        );
    }
}