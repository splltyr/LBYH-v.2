using UnityEngine;

public class FolderProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var health = collision.GetComponent<PlayerHealth>() ?? collision.GetComponentInParent<PlayerHealth>();
            var knight = collision.GetComponent<KnightHero>() ?? collision.GetComponentInParent<KnightHero>();

            if (health != null) health.TakeDamage((int)damage);
            if (knight != null) knight.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
