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
    public KnightHero playerScript; 
    public float normalSpeed = 10f;

    [Header("Audio")]
    public AudioSource earthquakeSound;
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float bgmVolume = 0.3f;
    private AudioSource bgmSource;

    private int index = 0;
    private bool isWakingUp = true; 

    [Header("Dialogue Sequence")]
    public LBYH_Line[] fullClinicDialogue = new LBYH_Line[] {
        new LBYH_Line { name = "Yves", text = "TALA!" },
        new LBYH_Line { name = "???", text = "Easy there, you’ve been out for almost a few hours. What were you doing out there? Are you trying to get killed?" },
        new LBYH_Line { name = "Yves", text = "What… Where am I?" },
        new LBYH_Line { name = "???", text = "Clinic. Isn’t it quite obvious? Did you hit your head or something?" },
        new LBYH_Line { name = "Yves", text = "I… I don’t know. Who are you?" },
        new LBYH_Line { name = "???", text = "Hey kid, are you aware of what’s going o- … You know what, nevermind. I’m Loren, the head Nurse here at STI." },
        new LBYH_Line { name = "Yves", text = "Nurse Loren? Where did you find me?" },
        new LBYH_Line { name = "Nurse Loren", text = "The outskirts of the school. Good thing I was out for supplies, you know? You were all bloody and everything. What happened? Did you… encounter him?" },
        new LBYH_Line { name = "Yves", text = "Him…" },
        new LBYH_Line { name = "Yves", text = "Agh.. My head.. I.. need to go." },
        new LBYH_Line { name = "Nurse Loren", text = "Oh thank Tala, it’s not like I can ask you to stay but, I think I can help you. Here, have this. (Bandage-1)" },
        new LBYH_Line { name = "Yves", text = "What do you mean? And who’s Tala?" },
        new LBYH_Line { name = "Nurse Loren", text = "I was out for supplies earlier today, remember? I found these tools abandoned. And I thought you knew who Tala is? Judging from you screaming Her name earlier." },
        new LBYH_Line { name = "Yves", text = "Thank you but I.. I don’t remember.. But It feels like.. She’s here." },
        new LBYH_Line { name = "Nurse Loren", text = "Tala doesn’t exist kid, but a few believed that She’s.. some kind of God or something. But for me, I think she’s just a guide who wants to help people in need." },
        new LBYH_Line { name = "Nurse Loren", text = "Enough of this. Go ahead and pick. You can always come back for more. I’m glad to be able to help you." },
        new LBYH_Line { name = "Yves", text = "That’s.. Quite a story. Why are you helping me?" },
        new LBYH_Line { name = "Nurse Loren", text = "Well, you.. remind me of someone." },
        new LBYH_Line { name = "Yves", text = "Someone?" },
        new LBYH_Line { name = "ACTION", text = "[EARTHQUAKE_START]" }, 
        new LBYH_Line { name = "Nurse Loren", text = "QUICK! Pick your weapon, Yves!" },
        new LBYH_Line { name = "Yves", text = "Wait, how did you know my na-" },
        new LBYH_Line { name = "ACTION", text = "[CEILING_CRASH]" }, 
        new LBYH_Line { name = "Yves", text = "Are you okay?!" },
        new LBYH_Line { name = "Nurse Loren", text = "I’m alright, just go! It’s dangerous!" },
        new LBYH_Line { name = "Yves", text = "I can’t just leave you there!" },
        new LBYH_Line { name = "Nurse Loren", text = "Don’t worry about me, as long as you keep moving on, you’ll find the exit! I know you’ll be great. Good luck, kid!" },
        new LBYH_Line { name = "ACTION", text = "[NURSE_EXIT_RIGHT]" },
        new LBYH_Line { name = "Yves", text = "She’s right.. I have to keep moving." },
        new LBYH_Line { name = "Yves", text = "Good thing there’s proper electricity here, it’s so dark.." },
        new LBYH_Line { name = "???", text = "Hehe, you’re funny, Yves." },
        new LBYH_Line { name = "Yves", text = "AAHH! A G-GHOST!" },
        new LBYH_Line { name = "Tala", text = "Yves, wait! We’ve been through this 6-7 times!" },
        new LBYH_Line { name = "ACTION", text = "[TALA_CHASE_START]" }
    };

    private AudioSource voiceAudioSource;

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (pressSpacePrompt != null) pressSpacePrompt.SetActive(false);
        if (screenFade != null) screenFade.alpha = 0;
        
        if (playerScript != null) 
        {
            playerScript.enabled = false;
            Rigidbody2D rb = playerScript.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            Animator anim = playerScript.GetComponent<Animator>();
            if (anim != null) anim.Play("KnightIdle");
        }
        
        // Setup and play BGM
        if (bgmClip != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

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

        while (index < fullClinicDialogue.Length)
        {
            LBYH_Line currentLine = fullClinicDialogue[index];

            // Logic Triggers (Non-dialogue)
            if (currentLine.text == "[EARTHQUAKE_START]") {
                StartCoroutine(TriggerEarthquake(false));
                index++; continue;
            }
            if (currentLine.text == "[CEILING_CRASH]") {
                StartCoroutine(TriggerEarthquake(true));
                index++; continue;
            }
            if (currentLine.text == "[NURSE_EXIT_RIGHT]") {
                yield return StartCoroutine(MoveNurse(ExitPoint.position, true)); 
                
                // ACTIVATE ARROW HERE
                if (exitArrow != null) exitArrow.SetActive(true); 
                
                index++; continue;
            }
            if (currentLine.text == "[TALA_CHASE_START]") {
                yield return StartCoroutine(TalaAppearance()); 
                break;
            }

            // Show Dialogue
            if (dialogueBox != null) dialogueBox.SetActive(true);

            // Update Name Plate (Assuming nameDisplay is added. Wait, Clinic uses ??? in text string)
            // The original script didn't use a separate name display, it just typed "Yves: TALA!"
            // I'll format it back together or rely on the UI. The user's original array included the name in the text!
            // Wait, their original text was "Yves: TALA!". My LBYH_Line split it to name="Yves" and text="TALA!".
            // Since their ClinicDialogueManager DOES NOT have a nameDisplay reference, I will prepend the name!
            string fullText = currentLine.name + ": " + currentLine.text;

            // Play Audio Clip if assigned
            if (currentLine.voiceClip != null)
            {
                if (voiceAudioSource == null) 
                {
                    // Grab the existing Audio Source if it's already there!
                    voiceAudioSource = gameObject.GetComponent<AudioSource>();
                    if (voiceAudioSource == null) 
                    {
                        voiceAudioSource = gameObject.AddComponent<AudioSource>();
                        voiceAudioSource.playOnAwake = false;
                    }
                    
                    if (gameObject.GetComponent<AutoVolumeNormalizer>() == null)
                    {
                        gameObject.AddComponent<AutoVolumeNormalizer>();
                    }
                }
                
                voiceAudioSource.Stop();
                
                // BULLETPROOF VOLUME BOOST HACK
                // Since Unity caps AudioSource.volume at 1.0, we just play the clip multiple times 
                // simultaneously on the exact same frame to physically multiply the sound wave amplitude!
                int playCount = Mathf.Max(1, Mathf.CeilToInt(currentLine.volume));
                float volumePerClip = currentLine.volume / playCount;

                for (int i = 0; i < playCount; i++)
                {
                    voiceAudioSource.PlayOneShot(currentLine.voiceClip, volumePerClip);
                }
            }

            yield return StartCoroutine(TypeLine(fullText));
            
            if (pressSpacePrompt != null) pressSpacePrompt.SetActive(true);
            
            // Wait for input to advance (Spammable to skip? Let's just do standard advance for now)
            while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButtonDown(0)) yield return null;
            
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

        if (playerScript != null) 
        {
            playerScript.enabled = true;
            playerScript.moveSpeed = normalSpeed * 1.8f; 
        }
        
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