using UnityEngine;
using System.Collections;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string characterName; 
    [TextArea(3, 10)] public string sentence;
}

public class RomeNPC : MonoBehaviour
{
    [Header("Movement & Appearance")]
    public float walkSpeed = 4f;
    public Transform walkToPoint; 

    [Header("UI Reference")]
    public GameObject dialogueBox;
    public TextMeshProUGUI textDisplay;
    public TextMeshProUGUI nameDisplay;
    public float typeSpeed = 0.04f;

    [Header("Dialogue Content Slots")]
    public DialogueLine[] initialEncounter; // Set this to 19 slots in Inspector
    public DialogueLine[] afterAwakening;   
    public DialogueLine[] merchantLines;    

    private bool isTalking = false;
    private bool isTyping = false; 
    private int index = 0;
    private DialogueLine[] currentArray;
    private bool hasMetYves = false;
    private Scene4Manager s4Manager;
    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        s4Manager = Object.FindAnyObjectByType<Scene4Manager>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isTalking && !isTyping)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                NextSentence();
            }
        }
    }

    public void StartConversation()
    {
        isTalking = true;
        index = 0;
        currentArray = !hasMetYves ? initialEncounter : merchantLines;

        if (dialogueBox != null) dialogueBox.SetActive(true);
        StartCoroutine(TypeSentence());
    }

    public void NextSentence()
    {
        if (index < currentArray.Length - 1)
        {
            index++;

            // Rome enters on line 2 (Index 1)
            if (currentArray == initialEncounter && index == 1)
            {
                StartCoroutine(AppearAndWalk());
            }

            // Cook Reya enters on line 10 (Index 9)
            if (currentArray == initialEncounter && index == 9)
            {
                if (s4Manager != null) s4Manager.ShowReya();
            }

            StartCoroutine(TypeSentence());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator AppearAndWalk()
    {
        if (col != null) col.isTrigger = true; 
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (walkToPoint != null)
        {
            transform.position = new Vector3(transform.position.x, walkToPoint.position.y, transform.position.z);
        }

        GetComponent<SpriteRenderer>().enabled = true;
        if (anim != null) anim.SetBool("RomeWalk", true); 

        while (walkToPoint != null && Vector2.Distance(transform.position, walkToPoint.position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, walkToPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = walkToPoint.position;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("RomeWalk", false);
    }

    IEnumerator TypeSentence()
    {
        isTyping = true;
        textDisplay.text = "";
        if (nameDisplay != null) nameDisplay.text = currentArray[index].characterName;

        foreach (char letter in currentArray[index].sentence.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    public void EndDialogue()
    {
        isTalking = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        
        if (col != null) col.isTrigger = false; 
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (s4Manager != null && s4Manager.playerObject != null)
        {
            var movement = s4Manager.playerObject.GetComponent<KnightHero>();
            if (movement != null) movement.enabled = true;
        }

        // Only trigger the Boss/Event if it's the first time meeting
        if (!hasMetYves && s4Manager != null)
        {
            s4Manager.StartBossEvent();
        }

        if (!hasMetYves) hasMetYves = true;
    }
}