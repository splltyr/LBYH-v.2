using UnityEngine;

/// <summary>
/// A self-building door trigger that is visible in the Editor.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class Scene5DoorZone : MonoBehaviour
{
    [SerializeField] private Scene5SequenceController sequence;
    private BoxCollider2D col;
    private SpriteRenderer doorRenderer;

    void OnValidate() 
    { 
        // Only do layout stuff here, NO sprite setting
        if (gameObject.layer == LayerMask.NameToLayer("UI"))
            gameObject.layer = LayerMask.NameToLayer("Default");
        
        col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            if (col.size == Vector2.one) col.size = new Vector2(2f, 3f);
        }
    }
    
    void Awake() 
    { 
        Setup(); 
    }

    void Setup()
    {
        // Setup visuals only at runtime or when manually called
        doorRenderer = GetComponent<SpriteRenderer>();
        if (doorRenderer == null) doorRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        if (doorRenderer.sprite == null)
        {
            doorRenderer.sprite = CreateDefaultSprite();
            doorRenderer.sortingOrder = 5;
        }

        if (sequence == null)
            sequence = Object.FindAnyObjectByType<Scene5SequenceController>();
    }

    public void Initialize(Scene5SequenceController controller)
    {
        sequence = controller;
        SetEnabled(false); 
    }

    public void SetEnabled(bool enabled)
    {
        if (col != null) col.enabled = enabled;
        if (doorRenderer != null) doorRenderer.color = enabled ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!Application.isPlaying) return;
        
        Debug.Log($"Door touched by: {other.gameObject.name} (Tag: {other.tag})");

        if (sequence == null) return;
        if (col != null && !col.enabled) return;
        
        // Flex-check: Is it the player? (Tag or Layer or Name)
        bool isPlayer = other.CompareTag("Player") || other.name.Contains("Knight") || other.name.Contains("Yves");
        
        if (isPlayer)
        {
            Debug.Log("<color=green>PLAYER DETECTED! Smashing door...</color>");
            sequence.NotifyPlayerReachedDoor();
            if (doorRenderer != null) doorRenderer.color = Color.green;
        }
    }

    private Sprite CreateDefaultSprite()
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }
}
