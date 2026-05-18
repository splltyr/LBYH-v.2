using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoTrigger : MonoBehaviour
{
    [Header("Core Settings")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Tutorial";
    public float interactionRange = 2.5f;

    [Header("UI Prompt")]
    public GameObject interactPrompt; // Drag your 'Press E' Text or Canvas here
    public bool floatPrompt = true;   // Makes the text bob up and down
    public float floatSpeed = 3f;
    public float floatAmount = 0.2f;

    private Transform player;
    private bool hasPlayed = false;
    private Vector3 initialPromptPos;

    void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Setup Video Event
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;

        // Initialize UI Prompt
        if (interactPrompt != null)
        {
            initialPromptPos = interactPrompt.transform.localPosition;
            interactPrompt.SetActive(false); // Hide by default
        }
    }

    void Update()
    {
        if (hasPlayed || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactionRange)
        {
            // Show Prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                
                // Roguelite 'Juice': Bobbing effect
                if (floatPrompt)
                {
                    float newY = initialPromptPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
                    interactPrompt.transform.localPosition = new Vector3(initialPromptPos.x, newY, initialPromptPos.z);
                }
            }

            // Check for Interaction
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (interactPrompt != null) interactPrompt.SetActive(false);
                PlayVideo();
            }
        }
        else
        {
            // Hide Prompt if player walks away
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    void PlayVideo()
    {
        if (videoPlayer != null)
        {
            hasPlayed = true;
            
            // Disable player movement script while video plays
            var knight = player.GetComponent<KnightHero>();
            if (knight != null) 
            {
                knight.enabled = false;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                
                var anim = player.GetComponent<Animator>();
                if (anim != null) anim.Play("KnightIdle");
            }
            else 
            {
                var movement = player.GetComponent<MonoBehaviour>();
                if (movement != null) movement.enabled = false;
            }

            videoPlayer.Play();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        vp.Stop();
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}