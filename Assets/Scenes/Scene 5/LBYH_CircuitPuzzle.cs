using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LBYH_CircuitPuzzle : MonoBehaviour
{
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
        CreatePersistentSprite();
    }

    private void CreatePersistentSprite()
    {
        if (whiteSprite != null) return;
        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
    }

    [ContextMenu("Build WORLD Puzzle (Click this!)")]
    public void BuildWorldPuzzle()
    {
        CreatePersistentSprite();
        if (puzzleRoot != null) DestroyImmediate(puzzleRoot);
        
        puzzleRoot = new GameObject("CircuitPuzzle_Visuals");
        puzzleRoot.transform.SetParent(this.transform);
        puzzleRoot.transform.localPosition = Vector3.zero;

        Material unlitMat = new Material(Shader.Find("Sprites/Default"));

        // 1. GIGANTIC Background (Covers the whole screen)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(puzzleRoot.transform);
        bgObj.transform.localPosition = new Vector3(0, 0, 10f); 
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.material = unlitMat;
        bgRenderer.color = Color.black;
        bgRenderer.sortingLayerName = sortingLayerName;
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
                tileObj.transform.localPosition = new Vector3(startX + (x * spacing), startY - (y * spacing), 0);
                
                SpriteRenderer tileSR = tileObj.AddComponent<SpriteRenderer>();
                tileSR.sprite = whiteSprite;
                tileSR.material = unlitMat;
                tileSR.color = new Color(0.1f, 0.1f, 0.1f);
                tileSR.sortingLayerName = sortingLayerName;
                tileSR.sortingOrder = baseSortingOrder + 5;
                tileObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

                tileObj.AddComponent<BoxCollider2D>().size = Vector2.one;

                GameObject wireObj = new GameObject("Wire");
                wireObj.transform.SetParent(tileObj.transform);
                wireObj.transform.localPosition = new Vector3(0, 0, -0.5f);
                
                SpriteRenderer wSR = wireObj.AddComponent<SpriteRenderer>();
                wSR.sprite = whiteSprite;
                wSR.material = unlitMat;
                wSR.sortingLayerName = sortingLayerName;
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
        canvObj.transform.localPosition = new Vector3(0, 6f, -1f);
        Canvas c = canvObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingLayerName = sortingLayerName;
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

    void Start() { if (puzzleRoot != null) puzzleRoot.SetActive(false); }

    void Update()
    {
        if (!isPuzzleOpen || isSolved || puzzleRoot == null || !puzzleRoot.activeSelf) return;

        if (inputLocked)
        {
            inputLocked = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("<color=orange>MOUSE CLICK DETECTED by Puzzle Script!</color>");

            Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam == null) 
            {
                Debug.LogError("PUZZLE ERROR: No Camera found!");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            
            if (hit.collider == null)
            {
                Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
                hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            }

            if (hit.collider != null)
            {
                Debug.Log("<color=green>HIT COLLIDER: </color>" + hit.collider.name);
                if (hit.collider.name.StartsWith("Tile_"))
                {
                    int index = int.Parse(hit.collider.name.Replace("Tile_", ""));
                    RotateTile(index);
                }
            }
            else
            {
                Debug.Log("<color=red>CLICKED BUT NO COLLIDER FOUND.</color>");
            }
        }
    }

    public void OpenPuzzle()
    {
        this.enabled = true; 
        if (puzzleRoot == null) BuildWorldPuzzle();
        CreatePersistentSprite();
        
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            Debug.Log($"<color=cyan>Parenting Puzzle to Camera: {cam.name}</color>");
            puzzleRoot.transform.SetParent(cam.transform);
            puzzleRoot.transform.localPosition = new Vector3(0, 0, 5f); 
            puzzleRoot.transform.localRotation = Quaternion.identity;
            puzzleRoot.transform.localScale = Vector3.one;
        }

        puzzleRoot.SetActive(true);
        if (bgRenderer != null) bgRenderer.color = Color.black;
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
            puzzleRoot.SetActive(false); 
            puzzleRoot.transform.SetParent(this.transform); 
        }
    }
}
