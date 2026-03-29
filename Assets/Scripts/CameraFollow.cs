using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 7f;
    public float yOffset = 1f;
    public float lookAheadDistance = 2f;

    void LateUpdate()
    {
        float direction = Mathf.Sign(target.localScale.x);

        float targetX = target.position.x + lookAheadDistance * direction;
        float targetY = target.position.y + yOffset;

        Vector3 newPos = new Vector3(
            Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime),
            targetY,
            -10f
        );

        transform.position = newPos;
    }
}
