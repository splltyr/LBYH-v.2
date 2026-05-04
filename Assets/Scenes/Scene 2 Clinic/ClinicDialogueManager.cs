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
    public Transform targetPoint;   
    public float walkSpeed = 5f;    
    private float nurseScale = 1f; 

    [Header("Player Control")]
    public PlayerMovement playerScript; 
    public float normalSpeed = 10f;

    [Header("Audio")]
    public AudioSource earthquakeSound;

    private int index = 0;
    private bool isWakingUp = true; 

    private string[] dialogueLines = {
        "[Ha? Nasaan ako?]", 
        "Yves: TALA!",
        "???: Easy there, you’ve been out for almost a few hours.",
        "???: What the hell were you doing out there? Are you trying to get killed?",
        "Yves: What… Where am I?",
        "???: Clinic. Isn’t it quite obvious? Did you hit your head or something?",
        "Loren: I’m Loren, the head Nurse here at STI.",
        "Nurse Loren: You were all bloody. Did you… encounter him?",
        "Yves: Him…",
        "Nurse Loren: I found these tools abandoned... pick something that’ll help you.",
        "[Earthquake rumbles violently!]", 
        "Nurse Loren: Quick! pick your weapon, Yves!",
        "Yves: Wait how did you know my name-",
        "[The ceiling crashes down, separating you both.]", 
        "Nurse Loren: You know when to stop… find your answer…",
        "Yves: I… need to keep moving to get some answer…"
    };

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (pressSpacePrompt != null) pressSpacePrompt.SetActive(false);
        if (screenFade != null) screenFade.alpha = 0;

        if (nurseAnim != null) nurseAnim.gameObject.SetActive(true);
        
        // Freeze Yves
        if (playerScript != null) playerScript.SetSpeed(0f);

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (!isWakingUp && clinicSpotlight != null)
        {
            if (Random.value > 0.96f) clinicSpotlight.intensity = Random.Range(0.3f, 1.1f);
        }
    }

    IEnumerator PlaySequence()
    {
        if (clinicSpotlight != null) StartCoroutine(SpotlightWakeUp());

        // Tala's fixed Z-axis walk
        if (nurseAnim != null && targetPoint != null)
        {
            nurseAnim.transform.position = new Vector3(nurseAnim.transform.position.x, nurseAnim.transform.position.y, 0);
            Vector3 targetPos = new Vector3(targetPoint.position.x, targetPoint.position.y, 0);
            nurseAnim.SetBool("isWalking", true);

            while (Vector2.Distance(nurseAnim.transform.position, targetPos) > 0.1f)
            {
                float direction = targetPos.x > nurseAnim.transform.position.x ? nurseScale : -nurseScale;
                nurseAnim.transform.localScale = new Vector3(direction, nurseScale, 1);
                Vector3 nextPos = Vector3.MoveTowards(nurseAnim.transform.position, targetPos, walkSpeed * Time.deltaTime);
                nurseAnim.transform.position = new Vector3(nextPos.x, nextPos.y, 0);
                yield return null;
            }
            nurseAnim.SetBool("isWalking", false);
            nurseAnim.transform.position = targetPos;
        }

        while (index < dialogueLines.Length)
        {
            string currentLine = dialogueLines[index];

            // IMPROVED: Detect trigger lines by content rather than index
            if (currentLine.Contains("Earthquake"))
            {
                yield return StartCoroutine(TriggerEarthquake(true));
                index++;
                continue;
            }
            
            if (currentLine.Contains("ceiling crashes"))
            {
                yield return StartCoroutine(TriggerEarthquake(false));
                index++;
                continue;
            }

            if (currentLine.StartsWith("["))
            {
                index++;
                continue;
            }

            if (dialogueBox != null) dialogueBox.SetActive(true);
            yield return StartCoroutine(TypeLine(currentLine));
            
            if (pressSpacePrompt != null) pressSpacePrompt.SetActive(true);
            
            // Wait for Space Bar input
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            if (pressSpacePrompt != null) pressSpacePrompt.SetActive(false);
            index++;
        }

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (playerScript != null) playerScript.SetSpeed(normalSpeed);
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f);
        }
    }

    IEnumerator SpotlightWakeUp()
    {
        isWakingUp = true; 
        clinicSpotlight.intensity = 40f; 
        float currentIntensity = 40f;
        while (currentIntensity > 1.0f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, 1.0f, Time.deltaTime * 1.2f);
            clinicSpotlight.intensity = currentIntensity;
            yield return null;
        }
        isWakingUp = false; 
    }

    IEnumerator TriggerEarthquake(bool playParticles)
    {
        // DEBUG: Check if audio is missing
        if (earthquakeSound != null) 
        {
            earthquakeSound.Play();
        }
        else 
        {
            Debug.LogWarning("Earthquake AudioSource is missing in the Inspector!");
        }

        if (playParticles && ceilingDebris != null) ceilingDebris.Play();

        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        float duration = 2.0f; // Increased for better effect

        while (elapsed < duration)
        {
            float x = Random.Range(-0.3f, 0.3f);
            float y = Random.Range(-0.3f, 0.3f);
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }

    public void StartFinalExit() { StartCoroutine(FinalFadeOut()); }

    IEnumerator FinalFadeOut()
    {
        if (screenFade == null) yield break;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            screenFade.alpha = t;
            yield return null;
        }
        SceneManager.LoadScene("Map2_GroundFloor");
    }
}