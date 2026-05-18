using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class DamonPrologue : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;       
    public GameObject playerObject; 
    public float baseSpeed = 5f;     // The speed he starts at
    public float maxSpeed = 15f;     // The speed he reaches
    public float acceleration = 2f;  // How fast he speeds up
    public float stopDistance = 0.8f; 
    public float customScale = 2.0f;

    [Header("UI & Transition")]
    public CanvasGroup blackScreenGroup; 
    public Image whiteFlashImage;        
    public GameObject dialogueUI;        
    public TextMeshProUGUI dialogueText; 
    public string nextSceneName = "Map1_Clinic";

    [Header("Tala Timing")]
    public float typingSpeed = 0.08f; 
    public float waitAfterTala = 3.5f; 

    [Header("Voice Clips")]
    public AudioClip damonVoice;
    public AudioClip talaVoice;
    private AudioSource voiceAudioSource;

    [Header("Ghost Trail (Hard Offset)")]
    public float ghostDelay = 0.07f;
    public Color ghostColor = new Color(1f, 0f, 1f, 0.4f);
    public float ghostXOffset = 2.0f; 
    
    private float currentSpeed; // Tracks the ramping speed
    private float startY;
    private bool isDone = false;
    private bool hasTriggeredConfrontation = false;
    private Animator anim;
    private SpriteRenderer mySR;
    private float ghostTimer;

    void Start()
    {
        // DESTROY any rogue teleport triggers so only Damon handles the teleport!
        foreach (var t in FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None)) Destroy(t.gameObject);
        foreach (var t in FindObjectsByType<LevelLoader>(FindObjectsSortMode.None)) Destroy(t.gameObject);
        foreach (var t in FindObjectsByType<UniversalScenePortal>(FindObjectsSortMode.None)) Destroy(t.gameObject);

        startY = transform.position.y;
        currentSpeed = baseSpeed; // Initialize at starting speed
        anim = GetComponentInChildren<Animator>();
        mySR = GetComponentInChildren<SpriteRenderer>();

        if (blackScreenGroup != null) blackScreenGroup.alpha = 0;
        if (whiteFlashImage != null) whiteFlashImage.color = new Color(1, 1, 1, 0); 
        if (dialogueUI != null) dialogueUI.SetActive(false);
        
        if (scarySource != null) scarySource.Play();
        if (bloodRain != null)
        {
            try
            {
                // Safely probe a property of the ParticleSystem to ensure the underlying C++ native object is valid.
                // If it is a missing/destroyed serialized reference in a built player, accessing gameObject
                // will throw a catchable managed exception, preventing the native crash in Play().
                if (bloodRain.gameObject != null)
                {
                    bloodRain.Play();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DamonPrologue] bloodRain reference is invalid or missing in the built game. Skipping play. Error: {ex.Message}");
            }
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Kinematic; }
    }

    void Update()
    {
        if (player == null || isDone || hasTriggeredConfrontation) return;

        // --- GRADUAL MOVEMENT INCREASE ---
        // Increase currentSpeed until it hits maxSpeed
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

        float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, currentSpeed * Time.deltaTime);
        transform.position = new Vector3(targetX, startY, transform.position.z);
        
        if (anim != null) anim.SetBool("isWalking", true);
        
        float direction = (player.position.x > transform.position.x ? -1 : 1);
        transform.localScale = new Vector3(direction * customScale, customScale, 1);

        ghostTimer += Time.deltaTime;
        if (ghostTimer >= ghostDelay) 
        { 
            SpawnGhost(direction); 
            ghostTimer = 0;
        }

        if (Mathf.Abs(transform.position.x - player.position.x) < stopDistance) 
        {
            hasTriggeredConfrontation = true; 
            StartCoroutine(TheConfrontation());
        }
    }

    void SpawnGhost(float dir)
    {
        if (mySR == null || mySR.sprite == null) return;
        GameObject g = new GameObject("DamonGhost_Instance");
        float offsetMove = (dir == 1) ? ghostXOffset : -ghostXOffset;
        g.transform.position = new Vector3(transform.position.x + offsetMove, transform.position.y, transform.position.z);
        g.transform.localScale = transform.localScale;
        SpriteRenderer gs = g.AddComponent<SpriteRenderer>();
        gs.sprite = mySR.sprite;
        gs.color = ghostColor;
        gs.sortingLayerName = mySR.sortingLayerName;
        gs.sortingOrder = mySR.sortingOrder - 1;
        StartCoroutine(FadeGhost(gs));
    }

    IEnumerator FadeGhost(SpriteRenderer gs)
    {
        float t = 0;
        while (t < 0.5f) {
            t += Time.deltaTime;
            if (gs != null) gs.color = new Color(ghostColor.r, ghostColor.g, ghostColor.b, Mathf.Lerp(ghostColor.a, 0, t / 0.5f));
            yield return null;
        }
        if (gs != null) Destroy(gs.gameObject);
    }

    IEnumerator TheConfrontation()
    {
        isDone = true;
        if (playerObject != null) {
            Behaviour[] allScripts = playerObject.GetComponentsInChildren<Behaviour>();
            foreach (var s in allScripts) {
                if (s != null && s.gameObject != this.gameObject && !(s is Animator) && !(s is Collider2D)) s.enabled = false;
            }
            Rigidbody2D prb = playerObject.GetComponent<Rigidbody2D>();
            if (prb != null) { prb.linearVelocity = new Vector2(0f, prb.linearVelocity.y); }

            // Reset player animator parameters and play KnightIdle animation so they don't keep running
            Animator playerAnim = playerObject.GetComponentInChildren<Animator>();
            if (playerAnim != null) {
                playerAnim.SetBool("isWalking", false);
                playerAnim.Play("KnightIdle");
            }
        }
        if (anim != null) { anim.SetBool("isWalking", false); anim.Play("DamonIdle"); }
        yield return new WaitForSeconds(0.5f);
        if (dialogueUI != null) {
            dialogueUI.SetActive(true);

            if (damonVoice != null) 
            {
                if (voiceAudioSource == null) 
                {
                    GameObject voiceObj = new GameObject("DamonVoiceSource");
                    voiceObj.transform.SetParent(this.transform);
                    voiceAudioSource = voiceObj.AddComponent<AudioSource>();
                    voiceAudioSource.playOnAwake = false;
                    voiceObj.AddComponent<AutoVolumeNormalizer>();
                }
                voiceAudioSource.clip = damonVoice;
                voiceAudioSource.Play();
            }

            var typewriter = dialogueUI.GetComponentInChildren<TypewriterEffect>();
            if (typewriter != null) {
                typewriter.StartTyping();
                yield return new WaitUntil(() => typewriter.isFinished);
                yield return new WaitForSeconds(1.5f);
            }

            if (voiceAudioSource != null && voiceAudioSource.isPlaying) {
                yield return new WaitWhile(() => voiceAudioSource.isPlaying);
            }
            dialogueUI.SetActive(false);
        }
        if (anim != null) anim.Play("DamonHit");
        yield return new WaitForSeconds(0.3f); 
        if (hitSource != null) hitSource.Play();
        yield return new WaitForSeconds(0.6f); 
        if (blackScreenGroup != null) blackScreenGroup.alpha = 1;
        yield return new WaitForSeconds(3f);
        if (dialogueUI != null && dialogueText != null) {
            var img = dialogueUI.GetComponent<Image>();
            if (img != null) img.enabled = false;
            dialogueText.color = Color.yellow; 
            dialogueUI.SetActive(true); 

            if (talaVoice != null) 
            {
                if (voiceAudioSource == null) 
                {
                    GameObject voiceObj = new GameObject("DamonVoiceSource");
                    voiceObj.transform.SetParent(this.transform);
                    voiceAudioSource = voiceObj.AddComponent<AudioSource>();
                    voiceAudioSource.playOnAwake = false;
                    voiceObj.AddComponent<AutoVolumeNormalizer>();
                }
                voiceAudioSource.PlayOneShot(talaVoice);
            }

            yield return StartCoroutine(ManualType("???: Yves..? Yves! Can you hear me? You can still try again!"));
            yield return new WaitForSeconds(waitAfterTala);
        }
        if (whiteFlashImage != null) {
            float t = 0;
            while (t < 1.0f) {
                t += Time.deltaTime * 1.5f;
                whiteFlashImage.color = new Color(1, 1, 1, t);
                yield return null;
            }
        }
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ManualType(string msg) {
        dialogueText.text = "";
        foreach (char c in msg) {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    [Header("Audio & Visuals")]
    public AudioSource scarySource; 
    public AudioSource hitSource;
    public ParticleSystem bloodRain;
}