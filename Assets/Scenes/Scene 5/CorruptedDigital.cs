using UnityEngine;

public class CorruptedDigital : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 3f;
    public GameObject hitEffect;

    private Transform player;
    private Vector2 targetDir;

    void Start()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj == null) pObj = FindAnyObjectByType<KnightHero>()?.gameObject;
        
        if (pObj != null)
        {
            player = pObj.transform;
            targetDir = (player.position - transform.position).normalized;
            
            // Rotate to face the direction of flight
            float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            targetDir = transform.right;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(targetDir * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<KnightHero>() != null)
        {
            KnightHero kh = collision.GetComponent<KnightHero>();
            if (kh != null) kh.TakeDamage(damage);
            
            Explode();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
