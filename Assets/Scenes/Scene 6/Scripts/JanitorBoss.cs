using UnityEngine;
using System.Collections;

public class JanitorBoss : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;

    [Header("Stats")]
    public float maxHealth = 600f;
    public float moveSpeed = 4f; // Faster!
    public float attackRange = 6f; // Larger range
    public float bossScale = 1.5f;

    private Animator anim;
    private bool isAttacking = false;
    private bool isStunned = false;
    private bool isDead = false;

    void OnEnable() 
    {
        InitializeBoss();
    }

    void Start()
    {
        InitializeBoss();
    }

    void InitializeBoss()
    {
        anim = GetComponentInChildren<Animator>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        transform.localScale = new Vector3(bossScale, bossScale, 1f);
        
        if (anim != null) anim.Play("Idle");
        Debug.Log("BOSS WOKE UP!");
    }

    private void SnapToGround()
    {
        int layerMask = LayerMask.GetMask("Ground", "Default", "Terrain");
        if (layerMask == 0) layerMask = ~0;
        
        // Raycast from slightly above downwards to find ground level
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 2f), Vector2.down, 20f, layerMask);
        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    void Update()
    {
        if (isDead || isStunned || isAttacking || player == null) return;

        SnapToGround(); // Dynamically keep him on the ground!

        float distance = Vector2.Distance(transform.position, player.position);

        // FORCE MOVEMENT
        if (distance > attackRange)
        {
            MoveTowardPlayer();
        }
        else
        {
            StartCoroutine(PerformAttack());
        }
    }

    void MoveTowardPlayer()
    {
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        
        // Move horizontally
        transform.position += new Vector3(dir * moveSpeed * Time.deltaTime, 0, 0);
        SnapToGround(); // Ensure he doesn't float when moving
        
        // Animation
        if (anim != null) anim.Play("Walk");
        
        // Flip
        float currentScaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(currentScaleX * (dir > 0 ? -1 : 1), transform.localScale.y, 1);
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        Debug.Log("BOSS ATTACKING!");
        
        if (anim != null) anim.Play("VineJab");
        
        yield return new WaitForSeconds(0.8f); 

        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 2f)
        {
            player.SendMessage("TakeDamage", 35f, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(1f); 
        isAttacking = false;
        if (!isDead && anim != null) anim.Play("Idle");
    }

    // This is the function called by the player's sword
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        // If the health script isn't doing it, we do it here
        Debug.Log("Boss hit for: " + amount);
        
        if (amount > 10) // Any decent hit
        {
            // Optional: Add hit reaction
            // if (anim != null) anim.Play("Hit");
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        if (anim != null) anim.Play("Death");
        Debug.Log("BOSS DIED!");
    }
}
