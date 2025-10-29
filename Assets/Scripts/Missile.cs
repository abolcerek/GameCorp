using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 4f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        // launch upward on spawn
        if (rb != null)
            rb.linearVelocity = Vector2.up * speed;

        Invoke(nameof(Kill), lifetime);
    }

    void OnDisable() => CancelInvoke();

    void Kill() => Destroy(gameObject);

    void OnTriggerEnter2D(Collider2D other)
{
    // Debug message to see what the missile hits
    Debug.Log("Missile hit: " + other.name + " tag=" + other.tag);

    // Check if the object is an asteroid
    if (other.CompareTag("Asteroid"))
    {
        // TODO: add explosion effect here later

        // Destroy both the asteroid and the missile
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
//fart haha
} 
