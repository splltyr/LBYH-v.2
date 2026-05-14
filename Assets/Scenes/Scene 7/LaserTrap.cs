using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 15f;
    public float laserRange = 50f;
    public float beamThickness = 0.5f; // ADJUST THIS to match your sprite's thickness!
    public float damageCooldown = 0.5f;

    [Header("Visuals")]
    public SpriteRenderer beamSprite; 
    public LineRenderer lineRenderer; 
    
    [Header("Setup")]
    public Transform firePoint;

    private float lastDamageTime;

    void Start()
    {
        if (firePoint == null) firePoint = transform;
        if (lineRenderer != null) lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        Vector2 direction = firePoint.right;
        Vector2 origin = (Vector2)firePoint.position;

        // --- THE THICK BOXCAST ---
        // This shoots a box as wide as your laser sprite
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        RaycastHit2D hit = Physics2D.BoxCast(origin + direction * 0.1f, new Vector2(0.1f, beamThickness), angle, direction, laserRange);

        float currentRange = laserRange;
        GameObject hitObject = null;

        if (hit.collider != null)
        {
            // Skip the emitter
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
            {
                RaycastHit2D secondHit = Physics2D.BoxCast((Vector2)hit.point + direction * 0.1f, new Vector2(0.1f, beamThickness), angle, direction, laserRange);
                if (secondHit.collider != null)
                {
                    currentRange = Vector2.Distance(origin, secondHit.point);
                    hitObject = secondHit.collider.gameObject;
                }
            }
            else
            {
                currentRange = hit.distance + 0.1f;
                hitObject = hit.collider.gameObject;
            }
        }

        // --- UPDATE VISUALS ---
        if (beamSprite != null)
        {
            beamSprite.size = new Vector2(currentRange, beamSprite.size.y);
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + direction * currentRange);
        }

        // Apply Damage
        if (hitObject != null)
        {
            CheckForDamage(hitObject);
        }
    }

    void CheckForDamage(GameObject target)
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        var health = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        var knight = target.GetComponent<KnightHero>() ?? target.GetComponentInParent<KnightHero>();

        if (health != null || knight != null || target.CompareTag("Player"))
        {
            lastDamageTime = Time.time;
            Debug.Log($"<color=red><b>LASER HIT:</b> {target.name}</color>");
            if (health != null) health.TakeDamage((int)damage);
            if (knight != null) knight.TakeDamage(damage);
            target.SendMessageUpwards("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}




