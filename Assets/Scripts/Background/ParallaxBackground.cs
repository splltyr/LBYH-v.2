using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = follows camera exactly. 1 = stays still. 0.5 = moves half speed.")]
    [Range(0f, 1f)] public float parallaxEffect; 

    private Transform cam;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    void Start()
    {
        cam = Camera.main.transform;
        lastCameraPosition = cam.position;

        // Optional: This part helps with infinite looping if your background is a repeating tile
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
        // Calculate how much the camera moved since the last frame
        Vector3 deltaMovement = cam.position - lastCameraPosition;

        // Move the background based on the parallax multiplier
        // Multiplying by (1 - effect) makes the background move slower than the camera
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * parallaxEffect, 0);

        // Update the last camera position for the next frame
        lastCameraPosition = cam.position;

        // Optional: Infinite Scrolling logic
        // If the camera moves past the width of the sprite, jump the sprite forward/backward
        if (Mathf.Abs(cam.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cam.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cam.position.x + offsetPositionX, transform.position.y);
        }
    }
}