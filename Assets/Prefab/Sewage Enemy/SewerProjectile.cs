using UnityEngine;

public class SewerProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float damage = 20f;
    public float lifeTime = 3f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Setup(Vector3 shootDir)
    {
        direction = shootDir;
        
        // Rotate fireball to face the direction it's flying
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Try multiple ways to find the player
        bool isPlayer = other.CompareTag("Player") || other.gameObject.name.Contains("KnightHero");

        if (isPlayer)
        {
            Debug.Log("Fireball HIT Player!");
            
            // Try to send damage
            other.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            
            // Special check for your specific player script if SendMessage fails
            var playerMove = other.GetComponent<PlayerMovement>();
            if (playerMove != null)
            {
                // Assuming your PlayerMovement has a health system or TakeDamage
                // playerMove.TakeDamage(damage); 
            }

            Destroy(gameObject); // Explode on hit
        }
    }
}
