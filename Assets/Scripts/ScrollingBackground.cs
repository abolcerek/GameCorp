using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 2f;      // Speed of scrolling
    public float backgroundHeight = 10f; // Height of one background image

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
}
