using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scene8SequenceController : MonoBehaviour
{
    [Header("Dialogue Blocks")]
    [SerializeField] private LBYH_Line[] introLines;
    [SerializeField] private LBYH_Line[] ambushLines;
    [SerializeField] private LBYH_Line[] questioningLines;
    [SerializeField] private LBYH_Line[] flashbackLab1;
    [SerializeField] private LBYH_Line[] flashbackLab2;
    [SerializeField] private LBYH_Line[] flashbackRuined1;
    [SerializeField] private LBYH_Line[] flashbackRuined2;
    [SerializeField] private LBYH_Line[] endingLines;

    [Header("Cutscene References (Flashback)")]
    [SerializeField] private Transform labCameraTarget;
    [SerializeField] private Transform ruinedCameraTarget;
    [SerializeField] private Transform damonSprite;
    [SerializeField] private Transform talaHumanSprite;
    [SerializeField] private GameObject computerGlitchVFX;

    [Header("Audio & Music")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private AudioClip labFlashbackBGM;
    [SerializeField] private AudioClip ruinedFlashbackBGM;
    [SerializeField] private AudioClip glitchSound;

    [Header("UI References")]
    [SerializeField] private LBYH_Dialogue dialogueUI;
    [SerializeField] private UnityEngine.UI.Image screenOverlay; 
    [SerializeField] private string nextSceneName = "Scene 9"; 

    [Header("Scene Objects")]
    [SerializeField] private GameObject fileFlingersGroup;
    [SerializeField] private GameObject nextSceneElevator; // Enable this at the very end!

    [ContextMenu("Initialize Dialogue Text")]
    public void InitializeDialogue()
    {
        introLines = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[The elevator doors open. A cold blue light floods the room. Rows of server racks stretch endlessly, connected by glowing cables.]" },
            new LBYH_Line { name = "Yves", text = "… This feels like a maze." },
            new LBYH_Line { name = "Narrative", text = "[Laser-like beams labeled “TCP/IP” move across pathways.]" },
            new LBYH_Line { name = "Yves", text = "One wrong move and I’m fried… great." },
            new LBYH_Line { name = "Tala", text = "Oooh~ Lechon for dinner!" },
            new LBYH_Line { name = "Yves", text = "Really, Tala?" },
            new LBYH_Line { name = "Tala", text = "HEHE" }
        };

        ambushLines = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[Enemies appear: File Flingers throwing sharp folder-like projectiles.]" },
            new LBYH_Line { name = "Narrative", text = "[Yves doesn’t notice the incoming attack towards him.]" },
            new LBYH_Line { name = "Tala", text = "YVES! BEHIND YOU!" },
            new LBYH_Line { name = "Narrative", text = "[Yves is startled by Tala out of nowhere then he dodges the attack to counter attack.]" },
            new LBYH_Line { name = "Yves", text = "Woah, That was a close one. Thanks, Tala." },
            new LBYH_Line { name = "Tala", text = "*sighs* What are you without me?" },
            new LBYH_Line { name = "Yves", text = "Yeah yeah." }
        };

        questioningLines = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[Yves looks around the floor and suddenly has the courage to question her since the beginning at the clinic.]" },
            new LBYH_Line { name = "Yves", text = "…" },
            new LBYH_Line { name = "Tala", text = "Uh.. Are you gonna say something?" },
            new LBYH_Line { name = "Narrative", text = "[Yves looks at Tala and looks like he’s hesitating to say something.]" },
            new LBYH_Line { name = "Yves", text = "Uhhh… I have been holding this since we met in the clinic." },
            new LBYH_Line { name = "Tala", text = "Stop edging me and say it already!" },
            new LBYH_Line { name = "Narrative", text = "[Yves got courage and delight but continued speaking.]" },
            new LBYH_Line { name = "Yves", text = "Okay first question, how did you find me and how did I end up at the clinic with Nurse Loren?" },
            new LBYH_Line { name = "Tala", text = "You’ll know the other details later but I was the one who recused you from [REDACTED] and brought you somewhere Nurse Loren can see you." },
            new LBYH_Line { name = "Narrative", text = "[Yves was surprised and delighted because of why he was here and going forward to find some answers about this world.]" },
            new LBYH_Line { name = "Yves", text = "I don’t understand.." },
            new LBYH_Line { name = "Tala", text = "You will, soon." },
            new LBYH_Line { name = "Yves", text = "It just doesn’t make sense.." },
            new LBYH_Line { name = "Tala", text = "Quit your babbling, do you have any other questions?" },
            new LBYH_Line { name = "Narrative", text = "[Yves starts working while tala floats beside him while yves’s walking to start finding the next elevator.]" },
            new LBYH_Line { name = "Yves", text = "Give me a second." },
            new LBYH_Line { name = "Narrative", text = "[Yves thinks about many possible questions but then goes to the one he thinks the most of all.]" },
            new LBYH_Line { name = "Yves", text = "I’m pretty sure that you were once a person.. How did you end up like.. That? A glowy bubble." },
            new LBYH_Line { name = "Narrative", text = "[Tala was speechless and thought about the situation of the place.]" },
            new LBYH_Line { name = "Tala", text = "It all started at the Expo…" }
        };

        flashbackLab1 = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[Timeframe from 10 years of STI when [REDACTED] was a young bright student of STI.]" },
            new LBYH_Line { name = "Tala", text = "[REDACTED] was a hardworking student with big ideas and a prodigious amount of ICT." },
            new LBYH_Line { name = "Damon", text = "Tala! Where are the writers and artists? They need to finish the script to continue the game we’re developing." },
            new LBYH_Line { name = "Tala", text = "The writers and artists are working on it. Don’t be obsessive on the game, Redacted,,, You need some rest, isn’t that why I’m your partner? I’m looking after you, so you don’t overwork too much." },
            new LBYH_Line { name = "Tala", text = "That’s where [REDACTED] was being so obsessive to finish the game because of how much time he wasted on it… He needs some rest or his mind gets corrupted and collapses once the Expo starts. But one day at the laboratory…" },
            new LBYH_Line { name = "Tala", text = "You really won’t listen, all these food I gave you are untouched! Damon, I am seriously getting annoyed. Eat this and rest before something happens to you!" },
            new LBYH_Line { name = "Narrative", text = "[Damon ignores Tala and continues working on the game. A couple of minutes later, Damon replies to Tala.]" },
            new LBYH_Line { name = "Damon", text = "Tala, I’m almost done just give me about an hour-" },
            new LBYH_Line { name = "Narrative", text = "[Tala cuts Damon off his speaking to reply on it quickly.]" },
            new LBYH_Line { name = "Tala", text = "AN HOUR!? DAMON, YOU HAVEN’T EATEN ANYTHING THE WHOLE DAY! STOP WORKING ON THE GAME AND EAT!" },
            new LBYH_Line { name = "Tala", text = "That’s when the malware happens in the game… Damon is so obsessed with developing the game he doesn’t realize that the virus is already growing inside the game then… While we both argued with each other." },
            new LBYH_Line { name = "Narrative", text = "[Tala excitedly came over to Damon to show the progress of the game.]" },
            new LBYH_Line { name = "Tala", text = "Damon, have you seen the script? It matches our imaginations so we- Huh..? What’s this?" }
        };

        flashbackLab2 = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[Damon looks at his computer and rushes to remove the virus but the internet was too slow. The computer starts glitching slightly red.]" },
            new LBYH_Line { name = "Damon", text = "Wait.. WAIT! AGHH NO. STUPID WIFI, I WAS ALMOST THERE TO REMOVE THE VIRUS!" },
            new LBYH_Line { name = "Narrative", text = "[Damon started to type to counter the malware but in zero progress malware started to possess Damon.]" },
            new LBYH_Line { name = "Tala", text = "Hey.. H-HEY DAMON?!? WHAT’S HAPPENING TO YO-" }
        };

        flashbackRuined1 = new LBYH_Line[] {
            new LBYH_Line { name = "Narrative", text = "[They pass out, then they are suddenly on an alternate timeline where the STI College Caloocan is ruined because of the malware destroying the world. STI College Caloocan is the only fortress with the power to fix everything left]" },
            new LBYH_Line { name = "[REDACTED]", text = "AAGGHHH! THIS IS YOUR FAULT!" },
            new LBYH_Line { name = "Narrative", text = "[Damon then strangles Tala’s neck causing her to die of suffocation.]" }
        };

        flashbackRuined2 = new LBYH_Line[] {
            new LBYH_Line { name = "Tala", text = "D-Damon.. P-please.. I-I can’t.. B-breathe.. Damo-" },
            new LBYH_Line { name = "Damon", text = "Tala..? Wait, wait no. I-I can still fix this.. Tala, wake up please.. I can’t do this without you.. I’m sorry." },
            new LBYH_Line { name = "Narrative", text = "[After what corrupted Damon did to Tala, his emotions got to him and transformed into the most powerful corrupted virus that has ever existed.]" },
            new LBYH_Line { name = "[REDACTED]", text = "YOU DESERVE THAT FOR ALWAYS NAGGING ME." },
            new LBYH_Line { name = "Damon", text = "I should’ve listened to you.." },
            new LBYH_Line { name = "Narrative", text = "[Glitching the looks of Damon and REDACTED, the two speaks in one body but glitching into two.]" }
        };

        endingLines = new LBYH_Line[] {
            new LBYH_Line { name = "Tala", text = "That’s what happened to Damon and started this hell of a nightmare in STI." },
            new LBYH_Line { name = "Narrative", text = "[The timeframe stops and back from the present time.]" },
            new LBYH_Line { name = "Narrative", text = "[Yves passes the last trap from the maze to the next elevator.]" },
            new LBYH_Line { name = "Yves", text = "Don’t worry, tala. I.. I have a feeling that we can still bring back Damon." },
            new LBYH_Line { name = "Tala", text = "That’s.. Impossible, Yves. The only thing that you can do is defeat [REDACTED] but bring Damon back? He’s gone." },
            new LBYH_Line { name = "Tala", text = "I don’t think I've told you this, but in order to defeat him, you have to get the final source code from the Library. Then, you’ll inject the quantum computer on the Penthouse in order to stop things… And hopefully– go back to our original timeline." },
            new LBYH_Line { name = "Narrative", text = "[Screen fades.]" }
        };

        Debug.Log("Scene 8 Dialogue Initialized!");
    }

    void Start()
    {
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>();
        if (fileFlingersGroup != null) fileFlingersGroup.SetActive(false);
        if (nextSceneElevator != null) nextSceneElevator.SetActive(false); // Lock the elevator until cutscene ends!
        
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // Make sure the music loops!

        // Auto-initialize if empty!
        if (flashbackLab1 == null || flashbackLab1.Length == 0) InitializeDialogue();
        
        StartCoroutine(RunSceneSequence());
    }

    IEnumerator RunSceneSequence()
    {
        if (bgmSource != null && normalBGM != null) { bgmSource.clip = normalBGM; bgmSource.Play(); }

        // 1. Intro
        yield return PlayDialogue(introLines);

        // 2. Ambush (File Flingers)
        if (fileFlingersGroup != null) fileFlingersGroup.SetActive(true);
        yield return PlayDialogue(ambushLines);
        
        // Wait for player to defeat the ambush enemies
        yield return new WaitUntil(() => IsDead(fileFlingersGroup));

        // 3. Questions
        yield return PlayDialogue(questioningLines);

        // 4. Flashback Transition
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        Transform originalCamTarget = cam != null ? cam.target : null;
        bool originalBounds = cam != null ? cam.useBoundaries : false;

        yield return StartCoroutine(FlashbackTransition(true));
        
        // Disable CameraFollow entirely during the cutscene so it never tracks the player!
        if (cam != null) cam.enabled = false;

        // Snap Camera to Lab
        if (cam != null && labCameraTarget != null) 
        {
            cam.transform.position = new Vector3(labCameraTarget.position.x, labCameraTarget.position.y, -10f); // Instant Snap!
        }

        if (bgmSource != null && labFlashbackBGM != null) { bgmSource.clip = labFlashbackBGM; bgmSource.Play(); }

        yield return PlayDialogue(flashbackLab1);

        // Computer Glitches!
        if (computerGlitchVFX != null) computerGlitchVFX.SetActive(true);
        if (bgmSource != null && glitchSound != null) bgmSource.PlayOneShot(glitchSound);
        yield return PlayDialogue(flashbackLab2);

        // Transition to Black Screen (Ruined STI)
        yield return StartCoroutine(FadeToBlack(true)); 

        // Hide lab VFX
        if (computerGlitchVFX != null) computerGlitchVFX.SetActive(false);

        // Snap Camera to Ruined STI target
        if (cam != null && ruinedCameraTarget != null)
        {
            cam.transform.position = new Vector3(ruinedCameraTarget.position.x, ruinedCameraTarget.position.y, -10f); // Snap camera!
        }

        // Fade back in so the player can actually see the Ruined STI cutscene!
        yield return StartCoroutine(FadeToBlack(false));

        if (bgmSource != null && ruinedFlashbackBGM != null) { bgmSource.clip = ruinedFlashbackBGM; bgmSource.Play(); }

        // Play flashbackRuined1 with dynamic movement trigger!
        if (flashbackRuined1 != null && flashbackRuined1.Length >= 3)
        {
            // Line 1: Damon shouting "AAGGHHH! THIS IS YOUR FAULT!" (Skip the Line 0 Narrative!)
            dialogueUI.PresentLine(flashbackRuined1[1]);
            while (dialogueUI.IsTyping) yield return null;

            // Trigger Damon's movement immediately when he shouts! (Skip Line 2 Narrative, show movement instead!)
            if (damonSprite != null && talaHumanSprite != null)
            {
                // Move Damon to stand 1.2 units to the left of Tala
                Vector3 targetPos = new Vector3(talaHumanSprite.position.x - 1.2f, talaHumanSprite.position.y, damonSprite.position.z);
                StartCoroutine(MoveEntity(damonSprite, targetPos, 1.5f, "Walk", "Strangle"));
            }

            yield return WaitForInput();
            dialogueUI.HideDialoguePanel();
        }
        else
        {
            yield return PlayDialogue(flashbackRuined1);
        }

        yield return PlayDialogue(flashbackRuined2);

        // End of Flashback - Fade out to black to hide transition
        yield return StartCoroutine(FadeToBlack(true));
        if (bgmSource != null && normalBGM != null) { bgmSource.clip = normalBGM; bgmSource.Play(); }

        if (cam != null) 
        {
            cam.enabled = true;
            cam.target = originalCamTarget;
            cam.useBoundaries = originalBounds;
            if (originalCamTarget != null) cam.transform.position = new Vector3(originalCamTarget.position.x, originalCamTarget.position.y, -10f);
        }
        
        // Fade back in to the present day
        yield return StartCoroutine(FadeToBlack(false));

        // 5. Final Revelation
        yield return PlayDialogue(endingLines);
        
        // Open the elevator as a fallback option
        if (nextSceneElevator != null) nextSceneElevator.SetActive(true);

        // Smoothly fade to black and load the next scene!
        yield return StartCoroutine(FadeToBlack(true));
        Debug.Log("Scene 8 Sequence Complete. Transitioning to: " + nextSceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    bool IsDead(GameObject group)
    {
        if (group == null || !group.activeSelf) return true;
        
        var eHealths = group.GetComponentsInChildren<EnemyHealth>(true);
        if (eHealths.Length == 0) return true;

        foreach (var h in eHealths) 
        {
            if (h != null && h.currentHealth > 0f) return false;
        }
        return true;
    }

    IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (lines == null) yield break;
        for (int i = 0; i < lines.Length; i++)
        {
            // Skip narrative lines so the player only sees spoken dialogue!
            if (lines[i].name == "Narrative") continue;
            
            dialogueUI.PresentLine(lines[i]);
            while (dialogueUI.IsTyping) yield return null;
            yield return WaitForInput();
        }
        dialogueUI.HideDialoguePanel();
    }

    IEnumerator FlashbackTransition(bool active)
    {
        if (screenOverlay == null) yield break;
        
        Color targetColor = active ? new Color(0.1f, 0.1f, 0.3f, 0.4f) : new Color(0,0,0,0);
        float duration = 1.5f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            screenOverlay.color = Color.Lerp(screenOverlay.color, targetColor, t / duration);
            yield return null;
        }
        screenOverlay.color = targetColor;
    }

    IEnumerator FadeToBlack(bool active)
    {
        if (screenOverlay == null) yield break;
        
        Color targetColor = active ? new Color(0, 0, 0, 1f) : new Color(0, 0, 0, 0);
        float duration = 1.5f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            screenOverlay.color = Color.Lerp(screenOverlay.color, targetColor, t / duration);
            yield return null;
        }
        screenOverlay.color = targetColor;
    }

    IEnumerator WaitForInput()
    {
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) || 
            (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame));
    }

    IEnumerator MoveEntity(Transform entity, Vector3 targetPos, float duration, string walkAnimName, string endAnimName)
    {
        if (entity == null) yield break;
        
        Animator anim = entity.GetComponent<Animator>();
        if (anim == null) anim = entity.GetComponentInChildren<Animator>();
        
        SafePlayAnim(anim, walkAnimName);
        
        Vector3 startPos = entity.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            entity.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        entity.position = targetPos;
        
        SafePlayAnim(anim, endAnimName);
        
        // Trigger Tala's response if Damon is the one who moved
        if (entity == damonSprite && talaHumanSprite != null)
        {
            Animator talaAnim = talaHumanSprite.GetComponent<Animator>();
            if (talaAnim == null) talaAnim = talaHumanSprite.GetComponentInChildren<Animator>();
            SafePlayAnim(talaAnim, "Hurt"); 
        }
    }

    private void SafePlayAnim(Animator anim, string stateName)
    {
        if (anim == null || string.IsNullOrEmpty(stateName)) return;
        try
        {
            anim.Play(stateName);
        }
        catch (System.Exception)
        {
            // Fallback to avoid crashes if states are missing
        }
    }
}
