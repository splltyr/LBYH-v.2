using UnityEngine;

public class FileFlinger : MonoBehaviour
{
    public GameObject folderPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float projectileSpeed = 10f;
    public float detectionRange = 15f;

    private Transform player;
    private float nextFireTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (folderPrefab == null || firePoint == null) return;

        GameObject folder = Instantiate(folderPrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;
        
        Rigidbody2D rb = folder.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
            // Rotate folder to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            folder.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
