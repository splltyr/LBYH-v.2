using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 5f;
    public CanvasGroup blackScreen; // Drag your Black Image here!

    private float lockedY;
    private bool isEnding = false;

    void Start()
    {
        lockedY = transform.position.y;
        
        // Ensure the screen is clear when the game starts
        if (blackScreen != null) 
        {
            blackScreen.alpha = 0;
            blackScreen.blocksRaycasts = false;
        }

        // Lock physics so he doesn't fall
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (player == null || isEnding) return;

        // Move Damon toward Yves
        float newX = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, lockedY, transform.position.z);

        // Flip sprite to face player
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        transform.localScale = new Vector3(dir, 1, 1);

        // THE TRIGGER: Once he is 0.8 units away, start the fade
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance < 0.8f) 
        {
            StartCoroutine(ForceEndGame());
        }
    }

    IEnumerator ForceEndGame()
    {
        isEnding = true;

        // 1. INSTANT BLACK
        if (blackScreen != null) 
        {
            blackScreen.alpha = 1;
            blackScreen.blocksRaycasts = true;
        }

        // 2. WAIT (Dramatic pause in the dark)
        yield return new WaitForSeconds(2.5f);

        // 3. LOAD THE CLINIC
        SceneManager.LoadScene("Map1_Clinic");
    }

    // Keep these so EnemyHealth doesn't break
    public void DisableBoss() { this.enabled = false; }
    public void HitPlayer() { } 
    public void EndAttack() { }
}