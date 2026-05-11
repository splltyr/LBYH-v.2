using UnityEngine;
using System.Collections;

public class Scene4Manager : MonoBehaviour
{
    [Header("Character References")]
    public RomeNPC romeScript; 
    public GameObject reyaObject; 
    public GameObject playerObject; 

    [Header("Boss Configuration")]
    public GameObject misTankBoss; 
    public Transform bossSpawnPoint;

    [Header("Reya Movement")]
    public Transform reyaStopPoint;
    public Transform reyaExitPoint; // New: Where she walks to leave
    public float reyaWalkSpeed = 3f;

    void Start()
    {
        if (reyaObject != null) reyaObject.SetActive(false);
        if (misTankBoss != null) misTankBoss.SetActive(false);
        
        if (playerObject != null)
        {
            var movement = playerObject.GetComponent<KnightHero>();
            if (movement != null) movement.enabled = false;
            
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (romeScript != null) romeScript.StartConversation();
    }

    public void ShowReya()
    {
        if (reyaObject != null && !reyaObject.activeSelf) 
        {
            reyaObject.SetActive(true);
            StartCoroutine(ReyaSequence());
        }
    }

    IEnumerator ReyaSequence()
    {
        Animator anim = reyaObject.GetComponent<Animator>();
        Rigidbody2D rb = reyaObject.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        // 1. Walk In
        if (anim != null && HasParameter(anim, "ReyaWalk")) anim.SetBool("ReyaWalk", true);
        while (reyaStopPoint != null && Vector2.Distance(reyaObject.transform.position, reyaStopPoint.position) > 0.05f)
        {
            reyaObject.transform.position = Vector2.MoveTowards(reyaObject.transform.position, reyaStopPoint.position, reyaWalkSpeed * Time.deltaTime);
            yield return null;
        }
        if (anim != null && HasParameter(anim, "ReyaWalk")) anim.SetBool("ReyaWalk", false);

        // Wait until dialogue is almost done (you can trigger the exit later if you prefer)
        // For now, we will wait for the script to call StartBossEvent to handle her leaving
    }

    public void StartBossEvent()
    {
        StartCoroutine(BossIntro());
    }

    IEnumerator BossIntro()
    {
        // 1. Reya Leaves
        Animator anim = reyaObject.GetComponent<Animator>();
        if (anim != null && HasParameter(anim, "ReyaWalk")) anim.SetBool("ReyaWalk", true);
        
        while (reyaExitPoint != null && Vector2.Distance(reyaObject.transform.position, reyaExitPoint.position) > 0.05f)
        {
            reyaObject.transform.position = Vector2.MoveTowards(reyaObject.transform.position, reyaExitPoint.position, reyaWalkSpeed * Time.deltaTime);
            yield return null;
        }
        reyaObject.SetActive(false);

        // 2. Suspense Pause
        yield return new WaitForSeconds(2f);
        Debug.Log("(???) : Hey, you fools. Come here!");

        // 3. Spawn Boss
        if (misTankBoss != null)
        {
            misTankBoss.SetActive(true);
            misTankBoss.transform.position = bossSpawnPoint.position;
        }
    }

    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters) if (param.name == paramName) return true;
        return false;
    }
}