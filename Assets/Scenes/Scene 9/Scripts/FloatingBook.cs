using UnityEngine;

public class FloatingBook : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatAmplitude = 0.5f;
    public float rotationSpeed = 20f;

    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        // Random offset so all books don't move in perfect sync
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        // Gentle bobbing up and down
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed + offset) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Slow rotating
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
