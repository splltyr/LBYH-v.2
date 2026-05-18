using UnityEngine;

public class GlitchNPC : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Vector3 lastPos;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
        
        rb = GetComponent<Rigidbody2D>();
        lastPos = transform.position;
    }

    void Update()
    {
        if (anim == null) return;

        float speed = 0f;
        float directionX = 0f;

        // Check if moving via Rigidbody
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            speed = Mathf.Abs(rb.linearVelocity.x);
            directionX = rb.linearVelocity.x;
        }
        else
        {
            // Check if moving via Transform translation
            Vector3 delta = transform.position - lastPos;
            speed = delta.magnitude / Time.deltaTime;
            directionX = delta.x;
        }

        if (speed > 0.1f)
        {
            anim.Play("GlitchWalk");
            
            // Flip the sprite based on movement direction
            if (Mathf.Abs(directionX) > 0.01f)
            {
                // If Glitch's sprite naturally faces RIGHT, use this:
                // float sign = directionX > 0 ? 1f : -1f;
                
                // If Glitch's sprite naturally faces LEFT, use this:
                float sign = directionX > 0 ? -1f : 1f; 
                
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(sign * Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
            }
        }
        else
        {
            anim.Play("GlitchIdle");
        }

        lastPos = transform.position;
    }
}
