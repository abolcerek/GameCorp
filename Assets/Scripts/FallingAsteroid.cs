using UnityEngine;

public class FallingAsteroid : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

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
        else if (other.CompareTag("Player"))
        {
            // Handle collision with player (e.g., reduce player health)
            // Assuming the player has a script with a method called TakeDamage
            Player_Health player = other.GetComponent<Player_Health>();
            if (player != null)
            {
                player.TakeDamage(1); // Reduce player health by 1
            }
            Destroy(gameObject); // destroy the asteroid
        }
    }
}