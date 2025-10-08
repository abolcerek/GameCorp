using UnityEngine;

public class FallingAsteroid : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;

    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Use the variable instead of hard-coded -6
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to a bullet
        if (other.CompareTag("Bullet"))
        {
            Destroy(gameObject);      // destroy the asteroid
            Destroy(other.gameObject); // destroy the bullet
        }
    }
}