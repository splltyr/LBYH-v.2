using UnityEngine;
using System.Collections;
using TMPro;

public class CookReyaNPC : MonoBehaviour
{
    [Header("Detection")]
    public float interactionRange = 5f; 
    public LayerMask playerLayer;

    [Header("UI Reference")]
    public GameObject dialogueBox;     
    public TextMeshProUGUI textDisplay; 
    public TextMeshProUGUI nameDisplay; 
    public float typeSpeed = 0.04f;

    [Header("Movement")]
    public Transform movePoint; // The safe point she walks to after rescue
    public float walkSpeed = 2f;

    [Header("Dialogue Content")]
    [TextArea(3, 10)] public string[] rescueDialogue;    // Talk 1: After Skeletons die
    [TextArea(3, 10)] public string[] questIncomplete;   // Talk 2: If boss is still alive
    [TextArea(3, 10)] public string[] questComplete;     // Talk 3: After boss is dead

    private Scene3Manager sceneManager;
    private KnightHero playerScript; 
    private bool isTalking = false;
    private bool isTyping = false;
    private int index = 0;
    private string[] currentArray;

    void Start()
    {
        sceneManager = FindAnyObjectByType<Scene3Manager>();
    }

    void Update()
    {
        // Check if Yves is nearby
        bool isPlayerNearby = Physics2D.OverlapCircle(transform.position, interactionRange, playerLayer);
        
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Only allow interaction if we aren't already typing (Spam Protection)
            if (!isTalking) 
            {
                StartConversation();
            }
            else if (!isTyping) 
            {
                NextSentence();
            }
        }
    }

    void StartConversation()
    {
        // SAFETY: Only talk if the skeletons are already cleared
        if (sceneManager.currentState == Scene3Manager.SceneState.IntroChat || 
            sceneManager.currentState == Scene3Manager.SceneState.SkeletonAmbush) 
        {
            Debug.Log("Skeletons are still attacking! Reya is too scared to talk.");
            return;
        }

        isTalking = true;
        index = 0;

        // Freeze Yves so he doesn't walk away mid-chat
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<KnightHero>();
            if (playerScript != null) playerScript.enabled = false;
        }

        // Logic check for mission state to pick the right dialogue
        if (sceneManager.currentState == Scene3Manager.SceneState.Reward)
            currentArray = questComplete;
        else if (sceneManager.currentState == Scene3Manager.SceneState.HuntCooker)
            currentArray = questIncomplete;
        else
            currentArray = rescueDialogue;

        // CRITICAL: Ensure the chosen array actually has content
        if (currentArray == null || currentArray.Length == 0)
        {
            Debug.LogError("The Dialogue Array for " + sceneManager.currentState + " is empty!");
            EndDialogue();
            return;
        }

        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (nameDisplay != null) nameDisplay.text = "Cook Reya";

        // Trigger her walking animation/movement only during the first rescue
        if (sceneManager.currentState == Scene3Manager.SceneState.RescueReya)
            StartCoroutine(MoveToSafePoint());

        StartCoroutine(TypeSentence());
    }

    IEnumerator MoveToSafePoint()
    {
        if (movePoint == null) yield break;
        
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.Play("ReyaWalk");

        while (Vector2.Distance(transform.position, movePoint.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, movePoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (anim != null) anim.Play("ReyaIdle");
    }

    IEnumerator TypeSentence()
    {
        isTyping = true;
        textDisplay.text = "";

        // Standard typewriter effect
        foreach (char letter in currentArray[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        
        isTyping = false;
    }

    public void NextSentence()
    {
        // Go to next line if it exists
        if (index < currentArray.Length - 1)
        {
            index++;
            StartCoroutine(TypeSentence());
        }
        // Otherwise, close the conversation
        else 
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        if (playerScript != null) playerScript.enabled = true; // Unfreeze Yves
        if (dialogueBox != null) dialogueBox.SetActive(false);

        // Advance the game mission state
        if (sceneManager.currentState == Scene3Manager.SceneState.RescueReya)
        {
            sceneManager.currentState = Scene3Manager.SceneState.HuntCooker;
            
            // Wake up the boss now!
            if (sceneManager.corruptedCooker != null) 
                sceneManager.corruptedCooker.SetActive(true);
        }
    }
}