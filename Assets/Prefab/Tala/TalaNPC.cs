using UnityEngine;
using System.Collections;
using TMPro;

public class TalaNPC : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialogueBox;     
    public TextMeshProUGUI textDisplay; 
    public TextMeshProUGUI nameDisplay; 
    public float typeSpeed = 0.04f;

    [Header("Dialogue Content")]
    [TextArea(3, 10)] public string[] introDialogue;

    private Scene3Manager sceneManager;
    private KnightHero playerScript; 
    private bool isTalking = false;
    private bool isTyping = false;
    private int index = 0;

    void Start()
    {
        sceneManager = FindAnyObjectByType<Scene3Manager>();
        Invoke("StartIntroChat", 1.5f); 
    }

    void Update()
    {
        // LOCK: E only works if NOT currently typing
        if (isTalking && !isTyping && Input.GetKeyDown(KeyCode.E))
        {
            NextSentence();
        }
    }

    public void StartIntroChat()
    {
        if (introDialogue == null || introDialogue.Length == 0) return;

        isTalking = true;
        index = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<KnightHero>();
            if (playerScript != null) playerScript.enabled = false;
        }

        if (dialogueBox != null) dialogueBox.SetActive(true);
        StartCoroutine(TypeSentence());
    }

    IEnumerator TypeSentence()
    {
        // SAFETY: Don't run if UI references are missing
        if (textDisplay == null || nameDisplay == null) {
            Debug.LogError("Tala is missing UI slots in the Inspector!");
            yield break;
        }

        isTyping = true;
        textDisplay.text = "";
        nameDisplay.text = (index < introDialogue.Length - 1) ? "???" : "Tala";

        foreach (char letter in introDialogue[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    public void NextSentence()
    {
        if (index < introDialogue.Length - 1)
        {
            index++;
            StartCoroutine(TypeSentence());
        }
        else EndDialogue();
    }

    void EndDialogue()
    {
        isTalking = false;
        if (playerScript != null) playerScript.enabled = true;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (sceneManager != null) sceneManager.FinishIntroChat();
    }
}