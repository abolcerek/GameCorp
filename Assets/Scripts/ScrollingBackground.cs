using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 2f;      // Speed of scrolling
    public float backgroundHeight = 10f; // Height of one background image

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Move downward
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

        // If background goes below the camera view, move it back up
        if (transform.position.y < -backgroundHeight)
        {
            transform.position += new Vector3(0, 2 * backgroundHeight, 0);
        }
    }

    /// <summary>
    /// Change the background sprite (for transitions like black hole, boss fight)
    /// </summary>
    public void ChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer && newSprite)
        {
            spriteRenderer.sprite = newSprite;
            Debug.Log($"[ScrollingBackground] Changed sprite to: {newSprite.name}");
        }
    }

    /// <summary>
    /// Change scroll speed (useful for dramatic moments)
    /// </summary>
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
}