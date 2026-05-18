using UnityEngine;
using System.Collections;

public class Scene3Manager : MonoBehaviour
{
    public static Scene3Manager Instance { get; private set; }

    public GameObject corruptedCooker;
    public GameObject postBossArrow;
    public string nextSceneName = "";

    [Header("BGM Setup")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.3f;
    private AudioSource bgmSource;

    [Header("Dialogue System")]
    public LBYH_Dialogue dialogueUI;
    
    public bool bossDefeated = false;
    public bool hasRescuedReya = false;
    public bool hasFinishedQuest = false;
    private bool bossIntroPlayed = false;
    private bool isDialoguePlaying = false;
    
    public LBYH_Line[] fullIntroChatLines = {
        new LBYH_Line { name = "???", text = "Yves! L-Listen to me first.. I-I’m exhausted.." },
        new LBYH_Line { name = "???", text = "Finally.. You stopped.." },
        new LBYH_Line { name = "Yves", text = "W-What do you want? I’m not afraid of you.. What are you?!" },
        new LBYH_Line { name = "???", text = "Seriously? If you’re not afraid, how come you can’t even face me?" },
        new LBYH_Line { name = "???", text = "Woah, that’s cold.. Let me introduce myself. I am- Wait DID YOU HEAR A WOMAN’S SCREAM?" }
    };
    
    public LBYH_Line[] fullRescueReyaLines = {
        new LBYH_Line { name = "Cook Reya", text = "Thank you, young man! Phew, that scared me to death!" },
        new LBYH_Line { name = "Yves", text = "Are you okay, Ma’am? What happened?" },
        new LBYH_Line { name = "???", text = "That is one gushy armor, that would look awesome on you, Yves!" },
        new LBYH_Line { name = "Yves", text = "That’s disgusting.." },
        new LBYH_Line { name = "???", text = "What did you just say..?" },
        new LBYH_Line { name = "Yves", text = "*Confused* Didn’t you hear her say that?" },
        new LBYH_Line { name = "???", text = "Yves, it’s only you who can see me.." },
        new LBYH_Line { name = "Yves", text = "Oh.. Must’ve been the wind.. Are you okay, Ma’am?" },
        new LBYH_Line { name = "Cook Reya", text = "Yes, I am. Thank you! I’m Ms. Reya, a cafeteria worker here at STI. I was doing inventory and these hoards of pixels just blocked my distribution route! They took most of the stuff I was carrying!" },
        new LBYH_Line { name = "Yves", text = "Were you carrying anything important?" },
        new LBYH_Line { name = "???", text = "Obviously.." },
        new LBYH_Line { name = "Cook Reya", text = "The food rations we had for the month, and some keycards for the building. We can’t navigate through STI without it." },
        new LBYH_Line { name = "Yves", text = "Navigate through STI… Where did the pixels go?" },
        new LBYH_Line { name = "???", text = "I know where they are, I’ll take you there!" },
        new LBYH_Line { name = "Cook Reya", text = "I saw them camp near the end of the Gazebo! Why? Don’t tell me you're gonna try and fight them?!" },
        new LBYH_Line { name = "Yves", text = "It’s.. worth a try." },
        new LBYH_Line { name = "Cook Reya", text = "Please be careful!" },
        new LBYH_Line { name = "Yves", text = "Say, you haven’t told me your name yet glowy bubble." },
        new LBYH_Line { name = "???", text = "FINALLY! I am Ta- wait.. What did you just call me?" },
        new LBYH_Line { name = "Yves", text = "Glowy bubble." },
        new LBYH_Line { name = "???", text = "You.. OH! Look, we’ve arrived." }
    };

    public LBYH_Line[] shortReyaWaitLines = {
        new LBYH_Line { name = "Cook Reya", text = "Please be careful fighting that thing!" }
    };
    
    public LBYH_Line[] skeletonsAliveLines = {
        new LBYH_Line { name = "Yves", text = "I need to defeat these skeletons first!" }
    };

    public LBYH_Line[] cookerIntroLines = {
        new LBYH_Line { name = "Corrupted Cooker", text = "THESE RATIONS CAN LAST ME ONLY A FEW DAYS, I NEED TO STEAL MORE HAHA! THIS IS FUN!" },
        new LBYH_Line { name = "???", text = "Someone’s Crazy.." },
        new LBYH_Line { name = "Yves", text = "Hey!" },
        new LBYH_Line { name = "???", text = "You’re crazier.. Do you know that?" },
        new LBYH_Line { name = "Corrupted Cooker", text = "WHO DARES SNEAK UPON ME?!" },
        new LBYH_Line { name = "Yves", text = "I think you have something that I need." },
        new LBYH_Line { name = "Corrupted Cooker", text = "HA! WHAT A BRAVE YOUNGSTER! TRYING TO STEAL FROM ME? WHAT A JOKE!" },
        new LBYH_Line { name = "Corrupted Cooker", text = "HA! YOU’RE ALL BARK NO BITE!" },
        new LBYH_Line { name = "???", text = "There Yves! You almost got him" },
        new LBYH_Line { name = "Corrupted Cooker", text = "I THINK I’LL COOK YOU FOR BREAKFAST INSTEAD!" }
    };

    public LBYH_Line[] cookerDefeatedLines = {
        new LBYH_Line { name = "???", text = "That was awesome, Yves!" },
        new LBYH_Line { name = "Yves", text = "Thanks, you were.. Awesome too." },
        new LBYH_Line { name = "???", text = "Aaawwww" },
        new LBYH_Line { name = "Yves", text = "I take it back." },
        new LBYH_Line { name = "???", text = "HEY! No fair." }
    };

    public LBYH_Line[] fullQuestCompleteLines = {
        new LBYH_Line { name = "Cook Reya", text = "Thank God you’re back! Are you alright?" },
        new LBYH_Line { name = "Yves", text = "Yes Ma’am, I am. Here’s what I got after defeating the virus. I believe that these are yours." },
        new LBYH_Line { name = "Cook Reya", text = "Thanks for your help! As a token of my appreciation, here's the keycard for the faculty area! You can find a friend of mine that can help you!" },
        new LBYH_Line { name = "Yves", text = "There are more people on this campus? Isn’t it dangerous?" },
        new LBYH_Line { name = "Cook Reya", text = "Yup! But most of them are staying on the ground floor.. The building’s corrupted. Some floors are safe.. I think." },
        new LBYH_Line { name = "Cook Reya", text = "What do you mean? We can’t really leave this place… at least not yet… [REDACTED] trapped all of us here- Anyways. My friend is also a staff member here. Tell them I sent you and they’ll offer some help." },
        new LBYH_Line { name = "Yves", text = "Okay, Thank you Ms. Reya." },
        new LBYH_Line { name = "Cook Reya", text = "No problem kid. I hope you achieve whatever you need to do. This place isn’t exactly the safest. I just wish none of this ever happened." },
        new LBYH_Line { name = "Yves", text = "Who is this [REDACTED]? Wow, I can't even pronounce its name properly." },
        new LBYH_Line { name = "???", text = "You’ll meet him.. Eventually." }
    };

    public LBYH_Line[] shortQuestCompleteLines = {
        new LBYH_Line { name = "Cook Reya", text = "Go on, head to the faculty area! That keycard should let you through." }
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Found duplicate Scene3Manager on " + gameObject.name + "! Destroying it.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (dialogueUI == null) dialogueUI = FindAnyObjectByType<LBYH_Dialogue>(FindObjectsInactive.Include);
        if(corruptedCooker != null) corruptedCooker.SetActive(false);
        if(postBossArrow != null) postBossArrow.SetActive(false);

        // Setup and play BGM automatically
        if (bgmClip != null)
        {
            GameObject bgmGO = new GameObject("SceneBGM");
            bgmGO.transform.SetParent(transform);
            bgmSource = bgmGO.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // Auto-start intro
        StartCoroutine(PlayIntroChat());
    }

    IEnumerator PlayIntroChat()
    {
        yield return new WaitForSeconds(1.5f); // Wait for scene load
        yield return StartCoroutine(PlayDialogue(fullIntroChatLines));
    }

    public void InteractWithReya()
    {
        if (isDialoguePlaying) return;

        if (dialogueUI != null && !dialogueUI.IsVisible) 
        {
            StartCoroutine(ReyaDialogueSequence());
        }
    }

    IEnumerator ReyaDialogueSequence()
    {
        isDialoguePlaying = true;
        LBYH_Line[] linesToPlay = fullRescueReyaLines;

        // Check if skeletons are alive
        SkeletonEnemy[] skeletons = FindObjectsByType<SkeletonEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bool skeletonsAlive = false;
        foreach (var skeleton in skeletons)
        {
            if (skeleton != null && !skeleton.IsDead)
            {
                skeletonsAlive = true;
                break;
            }
        }

        if (skeletonsAlive)
        {
            linesToPlay = skeletonsAliveLines;
        }
        else if (hasFinishedQuest)
            linesToPlay = shortQuestCompleteLines;
        else if (bossDefeated) 
            linesToPlay = fullQuestCompleteLines;
        else if (hasRescuedReya) 
            linesToPlay = shortReyaWaitLines;

        if (linesToPlay == null || linesToPlay.Length == 0) 
        {
            isDialoguePlaying = false;
            yield break;
        }

        yield return StartCoroutine(PlayDialogue(linesToPlay));

        // Only rescue Reya if she was actually successfully rescued (i.e. skeletons are dead)
        if (!bossDefeated && !hasRescuedReya && !skeletonsAlive)
        {
            hasRescuedReya = true;
            if (corruptedCooker != null) corruptedCooker.SetActive(true);
        }
        else if (bossDefeated && !hasFinishedQuest)
        {
            hasFinishedQuest = true;
            if (postBossArrow != null) postBossArrow.SetActive(true);
            
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"Loading next scene: {nextSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
        
        isDialoguePlaying = false;
    }

    private IEnumerator PlayDialogue(LBYH_Line[] lines)
    {
        if (dialogueUI == null) yield break;
        
        // Freeze Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        KnightHero hero = null;
        if (player != null)
        {
            hero = player.GetComponent<KnightHero>();
            if (hero != null) hero.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            Animator anim = player.GetComponent<Animator>();
            if (anim != null) anim.Play("KnightIdle");
        }

        foreach (var line in lines)
        {
            dialogueUI.PresentLine(line);
            yield return new WaitForEndOfFrame();
            while (dialogueUI.IsTyping) yield return null;
            
            // Wait for Advance
            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0));
            
            yield return new WaitUntil(() =>
                !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Return) &&
                !Input.GetKey(KeyCode.E) && !Input.GetMouseButton(0));
                
            yield return new WaitForSeconds(0.1f);
        }
        
        dialogueUI.HideDialoguePanel();
        if (hero != null) hero.enabled = true; // Unfreeze
        isDialoguePlaying = false;
    }

    public void EnemyDefeated(string enemyType)
    {
        if (enemyType == "CorruptedCooker")
        {
            bossDefeated = true;
            Debug.Log("Boss defeated! Get the keycard from Reya.");
            
            // Auto-play the post-boss dialogue!
            if (dialogueUI != null && !dialogueUI.IsVisible) 
            {
                StartCoroutine(PlayDialogue(cookerDefeatedLines));
            }
        }
    }
    
    // Call this from a Unity Event Trigger when the player walks into the Boss Area!
    public void TriggerCookerIntro()
    {
        if (bossIntroPlayed) return;

        if (!bossDefeated && hasRescuedReya && dialogueUI != null && !dialogueUI.IsVisible)
        {
            bossIntroPlayed = true;
            StartCoroutine(PlayDialogue(cookerIntroLines));
        }
    }
}