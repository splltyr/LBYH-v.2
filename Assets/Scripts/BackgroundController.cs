using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    // FIX 1: startPos and length must be separated with a type (float)
    private float startPos;
    private float length; 
    
    // public GameObject cam; // This line can be removed as we use cameraTransform
    public float parallaxEffect;

    // We'll change this to Transform for better performance and easier inspector setup
    [SerializeField] public Transform cameraTransform; // Using [SerializeField] makes it visible

    void Start()
    {
        startPos = transform.position.x;
        // FIX 2: Corrected GetCompontent<spriteRenderer> to GetComponent<SpriteRenderer>
        // FIX 3: Added the length variable to the Start() method
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        
        // If cameraTransform is not set, try to find the Main Camera
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
    }

    void LateUpdate()
    {
        // Check if the camera reference is valid before proceeding
        if (cameraTransform == null)
            return;

        // --- Parallax Movement ---
        
        // Calculates the distance the camera has moved relative to the starting position
        float temp = (cameraTransform.position.x * (1 - parallaxEffect));
        
        // Calculates the position change based on the parallax effect
        float distance = cameraTransform.position.x * parallaxEffect; 

        // Apply the new position
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        // --- Seamless Tiling Logic ---
        
        // FIX 4: Corrected variable name from 'movment' to 'temp' (or 'movement' if preferred)
        // Checks if the camera has moved far enough to the right to reset the background
        if(temp > startPos + length)
        {
            // FIX 5: Use += operator to correctly update startPos
            startPos += length;
        }
        // Checks if the camera has moved far enough to the left to reset the background
        else if (temp < startPos - length)
        {
            // FIX 5: Use -= operator to correctly update startPos
            startPos -= length;
        }
    }
}