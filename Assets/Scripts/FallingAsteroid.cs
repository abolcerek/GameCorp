using UnityEngine;

public class FallingAsteroid : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 5f;
    public float destroyY = -6f;

    [Header("Asteroid Stats")]
    public int health = 1;

    [Header("Asteroid Prefabs")]
    public GameObject mediumAsteroidPrefab; // ✅ assign in Inspector (only needed for large)

    void Update()
    {
        // Defensive check — prevents null reference
        if (GameManager.Instance == null || GameManager.Instance.isGameOver) 
            return;

        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) 
            return;

        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            TakeDamage(1);
        }
        else if (other.CompareTag("Player"))
        {
            Player_Health player = other.GetComponent<Player_Health>();
            if (player != null)
                player.TakeDamage(1);

            Destroy(gameObject);
        }
    }


    void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            // ✅ If this is a large asteroid, spawn 2 medium ones
            if (mediumAsteroidPrefab != null && gameObject.name.Contains("Large"))
            {
                SpawnMediumAsteroids();
            }

            Destroy(gameObject);
        }
    }

    void SpawnMediumAsteroids()
    {
        Vector3 pos1 = transform.position + new Vector3(-0.5f, 0.3f, 0f);
        Vector3 pos2 = transform.position + new Vector3(0.5f, 0.3f, 0f);

        GameObject obj1 = Instantiate(mediumAsteroidPrefab, pos1, Quaternion.identity);
        GameObject obj2 = Instantiate(mediumAsteroidPrefab, pos2, Quaternion.identity);

        Rigidbody2D rb1 = obj1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = obj2.GetComponent<Rigidbody2D>();
        if (rb1) rb1.linearVelocity = new Vector2(-1f, -3f);
        if (rb2) rb2.linearVelocity = new Vector2(1f, -3f);
    }

}
