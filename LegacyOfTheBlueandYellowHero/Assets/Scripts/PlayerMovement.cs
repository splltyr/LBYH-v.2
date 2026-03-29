using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 5f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    
    // Variable for sprint speed
    private float runSpeedMultiplier = 1.5f; 
    
    // Flag to manage attack state
    [HideInInspector] public bool isAttacking = false; 

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Animator animator;

    // Jumps
    private int jumpCount = 0;
    private int maxAirJumps = 1; 

    void Start()
    {
        // Ensure attack flag is false on start
        isAttacking = false;
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        bool grounded = CheckGrounded();
        
        // --- Attack Input ---
        // REMOVED: && grounded, allowing attack while running or airborne
        if (Input.GetMouseButtonDown(0) && !isAttacking) 
        {
            isAttacking = true;
            animator.SetTrigger("Attack"); 
        }

        // --- Jump Reset ---
        if (grounded)
        {
            jumpCount = 0;
        }

        // --- Jump Logic (Ensures Double Jump) ---
        if (Input.GetButtonDown("Jump") && !isAttacking) 
        {
            if (grounded)
            {
                // FIX: Use rb.velocity
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                animator.SetTrigger("Jump"); 
            }
            else if (jumpCount < maxAirJumps)
            {
                // FIX: Use rb.velocity
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                jumpCount++; 
                animator.SetTrigger("Jump");
            }
        }

        // --- Variable Jump Height ---
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            // FIX: Use rb.velocity
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // --- Animation updates ---
        
        // 1. Sends vertical velocity for Jump/Fall transitions (yVelocity)
        // FIX: Use rb.velocity.y
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        
        // 2. Sends absolute horizontal speed for Idle/Run/Walk transitions (Magnitude)
        animator.SetFloat("Magnitude", Mathf.Abs(horizontal)); 
        
        // 3. Controls Walk/Run transition based on Shift key
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && horizontal != 0;
        animator.SetFloat("RunMultiplier", isSprinting ? 1.0f : 0.0f); 

        Flip();
    }

    private void FixedUpdate()
    {
        // --- Sprint Logic ---
        float currentSpeed = speed;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && horizontal != 0;
        
        if (isSprinting)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // Horizontal movement: Always apply speed regardless of attack state
        // FIX: Use rb.velocity
        // REMOVED: The 'if (!isAttacking) / else' block, allowing full movement during attack!
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
    }

    private bool CheckGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            0.5f,
            groundLayer
        );
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0 || !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
    
    // Function MUST be called by an Animation Event on your Attack and RunAttack clips!
    public void EndAttack()
    {
        isAttacking = false;
    }
}