using UnityEngine;

/// <summary>
/// Smooth follow behind the player. Assign <see cref="playerTransform"/> to Yves (KnightHero transform),
/// or leave null to auto-find <see cref="KnightHero"/> at runtime.
/// Do not run together with KnightHero's built-in tala Lerp — KnightHero skips its lerp when this component exists on Tala.
/// </summary>
public class TalaFollow : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Yves / Knight root transform. If empty, first KnightHero in the scene is used.")]
    public Transform playerTransform;
    public Vector3 offset = new Vector3(-1.5f, 1.5f, 0f);

    [Header("Movement Settings")]
    public float smoothTime = 0.3f;

    Vector2 smoothVel2;

    void Awake()
    {
        if (playerTransform == null)
        {
            KnightHero hero = FindAnyObjectByType<KnightHero>(FindObjectsInactive.Exclude);
            if (hero != null)
                playerTransform = hero.transform;
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null)
            return;

        float direction = Mathf.Abs(playerTransform.localScale.x) < 0.01f
            ? 1f
            : Mathf.Sign(playerTransform.localScale.x);

        Vector3 targetPosition = playerTransform.position + new Vector3(offset.x * direction, offset.y, offset.z);

        Vector2 pos = transform.position;
        Vector2 tgt = new Vector2(targetPosition.x, targetPosition.y);
        Vector2 next = Vector2.SmoothDamp(pos, tgt, ref smoothVel2, smoothTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        transform.localScale = new Vector3(direction, 1f, 1f);
    }
}
