using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal; 
using System.Collections;
using UnityEngine.SceneManagement;

public class ClinicDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;
    public GameObject pressSpacePrompt; 

    [Header("Visual Effects")]
    public Light2D clinicSpotlight; 
    public ParticleSystem ceilingDebris; 
    public CanvasGroup screenFade; 

    [Header("Nurse Cutscene")]
    public Animator nurseAnim;      
    public Transform NurseStopPoint; 
    public Transform ExitPoint;      
    public float walkSpeed = 5f;    

    [Header("Exit Guidance")]
    public GameObject exitArrow; 

    [Header("Tala Settings")]
    public GameObject talaPrefab; 
    private GameObject spawnedTala;

    [Header("Player Control")]
    public PlayerMovement playerScript; 
    public float normalSpeed = 10f;

    [Header("Audio")]
    public AudioSource earthquakeSound;

    private int index = 0;
    private bool isWakingUp = true; 

    private string[] dialogueLines = {
        "Yves: TALA!",
        "???: Easy there, you’ve been out for almost a few hours. What were you doing out there? Are you trying to get killed?",
        "Yves: What… Where am I?",
        "???: Clinic. Isn’t it quite obvious? Did you hit your head or something?",
        "???: Hey kid, are you aware of what’s going o- … You know what, nevermind. I’m Loren, the head Nurse here at STI.",
        "Nurse Loren: The outskirts of the school. Good thing I was out for supplies, you know? You were all bloody and everything. What happened? Did you… encounter him?",
        "Yves: Him…",
        "Yves: Agh.. My head.. I.. need to go.",
        "Nurse Loren: Oh thank Tala, it’s not like I can ask you to stay but, I think I can help you. Here, have this. (Bandage-1)",
        "Yves: What do you mean? And who’s Tala?",
        "Nurse Loren: I was out for supplies earlier today, remember? I found these tools abandoned. And I thought you knew who Tala is?",
        "Yves: Thank you but I.. I don’t remember.. But It feels like.. She’s here.",
        "Nurse Loren: Tala doesn’t exist kid, but a few believed that She’s.. some kind of God or something.",
        "Nurse Loren: But for me, I think she’s just a guide who wants to help people in need. Enough of this. Go ahead and pick.",
        "Yves: That’s.. Quite a story. Why are you helping me?",
        "Nurse Loren: Well, you.. remind me of someone.",
        "[EARTHQUAKE_START]", 
        "Nurse Loren: QUICK! Pick your weapon, Yves!",
        "Yves: Wait, how did you know my na-",
        "[CEILING_CRASH]", 
        "Yves: Are you okay?!",
        "Nurse Loren: I’m alright, just go! It’s dangerous!",
        "Yves: I can’t just leave you there!",
        "Nurse Loren: Don’t worry about me, as long as you keep moving on, you’ll find the exit! Good luck, kid!",
        "[NURSE_EXIT_RIGHT]",
        "Yves: She’s right.. I have to keep moving.",
        "Yves: Good thing there’s proper electricity here, it’s so dark..",
        "???: Hehe, you’re funny, Yves.",
        "Yves: AAHH! A G-GHOST!",
        "[TALA_CHASE_START]"
    };

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (pressSpacePrompt != null) pressSpacePrompt.SetActive(false);
        if (screenFade != null) screenFade.alpha = 0;
        if (playerScript != null) playerScript.SetSpeed(0f);
        
        // Ensure arrow is hidden at the start
        if (exitArrow != null) exitArrow.SetActive(false); 

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (!isWakingUp && clinicSpotlight != null)
        {
            if (Random.value > 0.96f) clinicSpotlight.intensity = Random.Range(0.6f, 1.2f);
        }
    }

    IEnumerator PlaySequence()
    {
        if (clinicSpotlight != null) StartCoroutine(SpotlightWakeUp());

        if (nurseAnim != null && NurseStopPoint != null)
        {
            yield return StartCoroutine(MoveNurse(NurseStopPoint.position)); 
        }

        while (index < dialogueLines.Length)
        {
            string currentLine = dialogueLines[index];

            // Logic Triggers (Non-dialogue)
            if (currentLine == "[EARTHQUAKE_START]") {
                StartCoroutine(TriggerEarthquake(false));
                index++; continue;
            }
            if (currentLine == "[CEILING_CRASH]") {
                StartCoroutine(TriggerEarthquake(true));
                index++; continue;
            }
            if (currentLine == "[NURSE_EXIT_RIGHT]") {
                yield return StartCoroutine(MoveNurse(ExitPoint.position, true)); 
                
                // ACTIVATE ARROW HERE
                if (exitArrow != null) exitArrow.SetActive(true); 
                
                index++; continue;
            }
            if (currentLine == "[TALA_CHASE_START]") {
                yield return StartCoroutine(TalaAppearance()); 
                break;
            }

            // Show Dialogue
            if (dialogueBox != null) dialogueBox.SetActive(true);
            yield return StartCoroutine(TypeLine(currentLine));
            
            if (pressSpacePrompt != null) pressSpacePrompt.SetActive(true);
            while (!Input.GetKeyDown(KeyCode.Space)) yield return null;
            if (pressSpacePrompt != null) pressSpacePrompt.SetActive(false);
            index++;
        }
    }

    IEnumerator MoveNurse(Vector3 destination, bool isExit = false)
    {
        nurseAnim.SetBool("isWalking", true);
        float direction = destination.x > nurseAnim.transform.position.x ? 1 : -1;
        nurseAnim.transform.localScale = new Vector3(direction, 1, 1);

        while (Vector2.Distance(nurseAnim.transform.position, destination) > 0.15f)
        {
            nurseAnim.transform.position = Vector3.MoveTowards(nurseAnim.transform.position, destination, walkSpeed * Time.deltaTime);
            yield return null;
        }

        nurseAnim.SetBool("isWalking", false);
        nurseAnim.transform.position = destination;

        if (isExit) nurseAnim.gameObject.SetActive(false); 
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line) {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }

    IEnumerator SpotlightWakeUp()
    {
        isWakingUp = true;
        float currentIntensity = 40f;
        while (currentIntensity > 1.2f) {
            currentIntensity = Mathf.Lerp(currentIntensity, 1.2f, Time.deltaTime * 1.5f);
            clinicSpotlight.intensity = currentIntensity;
            yield return null;
        }
        isWakingUp = false;
    }

    IEnumerator TriggerEarthquake(bool playParticles)
    {
        if (earthquakeSound != null) earthquakeSound.Play();
        if (playParticles && ceilingDebris != null) ceilingDebris.Play();

        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        float duration = playParticles ? 2.5f : 1.5f;

        while (elapsed < duration) {
            float x = Random.Range(-0.2f, 0.2f);
            float y = Random.Range(-0.2f, 0.2f);
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }

    IEnumerator TalaAppearance()
    {
        if (talaPrefab != null && spawnedTala == null)
        {
            Vector3 spawnOffset = new Vector3(-1.5f, 1.5f, 0);
            spawnedTala = Instantiate(talaPrefab, playerScript.transform.position + spawnOffset, Quaternion.identity);
            
            TalaFollow follow = spawnedTala.GetComponent<TalaFollow>();
            if (follow != null) follow.playerTransform = playerScript.transform;
        }

        if (playerScript != null) playerScript.SetSpeed(normalSpeed * 1.8f); 
        
        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(FinalFadeOut());
    }

    IEnumerator FinalFadeOut()
    {
        if (screenFade == null) yield break;
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime;
            screenFade.alpha = t;
            yield return null;
        }
        SceneManager.LoadScene("Map2_GroundFloor");
    }

    public void StartFinalExit() { StartCoroutine(FinalFadeOut()); }
}