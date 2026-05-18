using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LBYH_CircuitPuzzle : MonoBehaviour
{
    // --- Eternal Checkbox Fix ---
    void OnValidate() { this.enabled = true; }
    [Header("Puzzle Configuration")]
    public int gridSide = 3; 
    public bool isSolved = false;
    public bool isPuzzleOpen = false;
    private bool inputLocked = true;  // blocks input for one frame after open

    [Header("Sorting")]
    public string sortingLayerName = "UI";
    public int baseSortingOrder = 30000;

    [Header("Visuals")]
    public GameObject puzzleRoot;
    public List<Transform> wireTransforms = new List<Transform>();
    public List<SpriteRenderer> wireRenderers = new List<SpriteRenderer>();
    private SpriteRenderer bgRenderer;
    
    private List<float> currentRotations = new List<float>();
    public System.Action OnPuzzleSolved;
    private Sprite whiteSprite;

    void Awake()
    {
        this.enabled = true; // Super-glue the checkbox!
        CreatePersistentSprite();
        RestoreMissingSprites();
    }

    void Start() 
    { 
        this.enabled = true; // Stay on!
        RestoreMissingSprites();
        if (puzzleRoot != null) 
        {
            puzzleRoot.SetActive(true); // Always checked!
            SetPuzzleAlpha(0f); // But invisible at the start of gameplay
        }
    }

    /// <summary>
    /// Restores the dynamically created whiteSprite to all SpriteRenderers at runtime.
    /// This is required because Unity cannot serialize dynamically created assets in scene/prefab files,
    /// causing the sprite references to become null/missing when play mode starts.
    /// </summary>
    public void RestoreMissingSprites()
    {
        CreatePersistentSprite();
        if (puzzleRoot == null) return;

        SpriteRenderer[] renderers = puzzleRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            if (sr.sprite == null)
            {
                sr.sprite = whiteSprite;
            }
        }
    }

    private Sprite LoadOrCreatePersistentSpriteAsset()
    {
#if UNITY_EDITOR
        string path = "Assets/white_pixel.png";
        if (!System.IO.File.Exists(path))
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("<color=green>LBYH Puzzle: Created persistent white_pixel.png asset successfully!</color>");
        }
        Sprite spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spr != null) return spr;
#endif
        // Fallback for runtime builds
        if (whiteSprite != null) return whiteSprite;
        Texture2D fallbackTex = new Texture2D(2, 2);
        fallbackTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        fallbackTex.Apply();
        whiteSprite = Sprite.Create(fallbackTex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
        return whiteSprite;
    }

    private void CreatePersistentSprite()
    {
        whiteSprite = LoadOrCreatePersistentSpriteAsset();
    }

    [ContextMenu("Build WORLD Puzzle (Click this!)")]
    public void BuildWorldPuzzle()
    {
        CreatePersistentSprite();
        if (puzzleRoot != null) 
        {
            if (Application.isPlaying) Destroy(puzzleRoot);
            else DestroyImmediate(puzzleRoot);
        }
        
        puzzleRoot = new GameObject("CircuitPuzzle_Visuals");
        puzzleRoot.transform.SetParent(this.transform);
        puzzleRoot.transform.localPosition = Vector3.zero;

        // Fallback to "Default" sorting layer since the project does not have a "UI" sorting layer defined
        string activeSortingLayer = string.IsNullOrEmpty(sortingLayerName) || sortingLayerName == "UI" ? "Default" : sortingLayerName;

        // 1. GIGANTIC Background (Covers the whole screen)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(puzzleRoot.transform);
        bgObj.transform.localPosition = new Vector3(0, 0, 1f); // Place 1 unit behind the tiles
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = Color.black;
        bgRenderer.sortingLayerName = activeSortingLayer;
        bgRenderer.sortingOrder = baseSortingOrder;
        bgObj.transform.localScale = new Vector3(2000, 2000, 1); 

        wireTransforms.Clear();
        wireRenderers.Clear();

        // 2. Grid Layout
        float startX = -3f;
        float startY = 3f;
        float spacing = 3f;

        for (int y = 0; y < gridSide; y++)
        {
            for (int x = 0; x < gridSide; x++)
            {
                int index = y * gridSide + x;
                GameObject tileObj = new GameObject("Tile_" + index);
                tileObj.transform.SetParent(puzzleRoot.transform);
                tileObj.transform.localPosition = new Vector3(startX + (x * spacing), startY - (y * spacing), 0f); // At Z = 0
                
                SpriteRenderer tileSR = tileObj.AddComponent<SpriteRenderer>();
                tileSR.sprite = whiteSprite;
                tileSR.color = new Color(0.1f, 0.1f, 0.1f);
                tileSR.sortingLayerName = activeSortingLayer;
                tileSR.sortingOrder = baseSortingOrder + 5;
                tileObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

                tileObj.AddComponent<BoxCollider2D>().size = Vector2.one;

                GameObject wireObj = new GameObject("Wire");
                wireObj.transform.SetParent(tileObj.transform);
                wireObj.transform.localPosition = new Vector3(0, 0, -0.1f); // Place slightly in front of the tile
                
                SpriteRenderer wSR = wireObj.AddComponent<SpriteRenderer>();
                wSR.sprite = whiteSprite;
                wSR.sortingLayerName = activeSortingLayer;
                wSR.sortingOrder = baseSortingOrder + 10;
                wireObj.transform.localScale = new Vector3(1.5f, 0.3f, 1f);
                wSR.color = (index % 3 == 0) ? Color.red : (index % 3 == 1) ? Color.green : Color.blue;
                
                wireTransforms.Add(wireObj.transform);
                wireRenderers.Add(wSR);
            }
        }

        // 3. GIANT Instructions
        GameObject canvObj = new GameObject("TutorialCanvas");
        canvObj.transform.SetParent(puzzleRoot.transform);
        canvObj.transform.localPosition = new Vector3(0, 6f, -0.2f); // Place slightly in front of the tiles/wires
        Canvas c = canvObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingLayerName = activeSortingLayer;
        c.sortingOrder = baseSortingOrder + 20;
        RectTransform rt = canvObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(20, 5);
        rt.localScale = Vector3.one;

        GameObject textObj = new GameObject("Instructions");
        textObj.transform.SetParent(canvObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = "PUZZLE ACTIVE: ALIGN ALL WIRES HORIZONTALLY\n(CLICK TILES TO ROTATE)";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.yellow;
        t.fontSize = 40; 
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(2000, 500);
        textObj.transform.localScale = new Vector3(0.01f, 0.01f, 1);

        Debug.Log("Full-Screen Puzzle Built!");
    }


    void Update()
    {
        // --- DIAGNOSTIC: This proves the script is alive ---
        if (Input.GetMouseButtonDown(0)) 
        {
            Debug.Log("<color=magenta>Puzzle: CLICK DETECTED! (IsOpen: " + isPuzzleOpen + ", IsSolved: " + isSolved + ")</color>");
            
            if (!isPuzzleOpen || isSolved) return;
            
            HandleClick();
        }
    }

    void HandleClick()
    {
        if (inputLocked)
        {
            inputLocked = false;
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) cam = FindAnyObjectByType<Camera>();
        
        Debug.Log($"<color=orange>Puzzle: Raycasting using Camera: {cam?.name ?? "NULL!"}</color>");
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        // --- PIERCING RAYCAST: Look through everything to find a Tile ---
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);
        
        bool foundTile = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                Debug.Log("<color=green>Puzzle: Hit </color>" + hit.collider.name);
                if (hit.collider.name.StartsWith("Tile_"))
                {
                    int index = int.Parse(hit.collider.name.Replace("Tile_", ""));
                    RotateTile(index);
                    foundTile = true;
                    break; // Stop once we find a tile
                }
            }
        }

        if (!foundTile)
        {
            Debug.Log("<color=red>Puzzle: Clicked, but NO TILE hit. (Found " + hits.Length + " other objects)</color>");
        }
    }

    public void OpenPuzzle()
    {
        this.enabled = true; 
        
        // Preserve editor-built puzzle if it exists! Only build if completely missing.
        if (puzzleRoot == null)
        {
            BuildWorldPuzzle();
        }
        CreatePersistentSprite();
        RestoreMissingSprites(); // Ensure all sprites are fully restored before opening!
        
        // Force all child SpriteRenderers to use a valid sorting layer if they are set to "UI"
        if (puzzleRoot != null)
        {
            SpriteRenderer[] renderers = puzzleRoot.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (sr.sortingLayerName == "UI" || string.IsNullOrEmpty(sr.sortingLayerName))
                {
                    sr.sortingLayerName = "Default";
                }
            }
        }
        
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null && puzzleRoot != null)
        {
            Debug.Log($"<color=cyan>Positioning Puzzle in front of Camera: {cam.name}</color>");
            
            // --- THE 100% FOOLPROOF CAMERA POSITIONING TRICK ---
            // Parent to camera, set perfectly centered local position, then unparent to freeze in world space.
            // This guarantees the puzzle is exactly 5 units in front of the camera, perfectly centered, 
            // regardless of camera rotation, scale, or parent constraints!
            puzzleRoot.transform.SetParent(cam.transform);
            puzzleRoot.transform.localPosition = new Vector3(0f, 0f, 5f);
            puzzleRoot.transform.localRotation = Quaternion.identity;
            puzzleRoot.transform.localScale = Vector3.one;
            puzzleRoot.transform.SetParent(null); // Unparent immediately to avoid any player scale flipping!
            
            // Dynamically retrieve the background renderer if it was built in the editor
            if (bgRenderer == null)
            {
                Transform bgTrans = puzzleRoot.transform.Find("Background");
                if (bgTrans != null) bgRenderer = bgTrans.GetComponent<SpriteRenderer>();
            }

            // Move background behind the tiles
            if (bgRenderer != null) bgRenderer.sortingOrder = baseSortingOrder - 10;
        }

        if (puzzleRoot != null)
        {
            puzzleRoot.SetActive(true);
            SetPuzzleAlpha(1f); // Make it visible!
        }
        if (bgRenderer != null) bgRenderer.color = new Color(0, 0, 0, 1f); // Ensure black background
        isSolved = false;
        isPuzzleOpen = true;
        inputLocked = true; 
        
        currentRotations.Clear();
        for (int i = 0; i < wireTransforms.Count; i++)
        {
            float rot = Random.Range(1, 4) * 90f;
            currentRotations.Add(rot);
            wireTransforms[i].localRotation = Quaternion.Euler(0, 0, rot);
        }
    }

    void RotateTile(int index)
    {
        if (isSolved) return;
        currentRotations[index] = (currentRotations[index] + 90f) % 360f;
        wireTransforms[index].localRotation = Quaternion.Euler(0, 0, currentRotations[index]);
        Debug.Log($"Tile {index} Rotated to: {currentRotations[index]}");
        CheckSolved();
    }

    void CheckSolved()
    {
        foreach (float rot in currentRotations)
        {
            float angleError = Mathf.Abs(Mathf.DeltaAngle(rot, 0));
            if (angleError > 0.1f && Mathf.Abs(angleError - 180) > 0.1f) return;
        }

        isSolved = true;
        Debug.Log("<color=green>PUZZLE SOLVED!</color>");
        if (bgRenderer != null) bgRenderer.color = new Color(0, 0.5f, 0, 0.8f); 
        foreach (var sr in wireRenderers) sr.color = Color.white;
        Invoke(nameof(Close), 1.5f);
    }

    void Close() 
    { 
        isPuzzleOpen = false;
        if (puzzleRoot != null) 
        {
            SetPuzzleAlpha(0f); // Hide it but keep it active
            puzzleRoot.transform.SetParent(this.transform); 
        }
    }

    private void SetPuzzleAlpha(float alpha)
    {
        if (puzzleRoot == null) return;

        // SpriteRenderers
        SpriteRenderer[] srs = puzzleRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // UI Text
        Text[] texts = puzzleRoot.GetComponentsInChildren<Text>(true);
        foreach (var t in texts)
        {
            Color c = t.color;
            c.a = alpha;
            t.color = c;
        }

        // Colliders - disable them when alpha is 0 so they don't block clicks
        Collider2D[] cols = puzzleRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var col in cols)
        {
            col.enabled = (alpha > 0.1f);
        }
    }
}
