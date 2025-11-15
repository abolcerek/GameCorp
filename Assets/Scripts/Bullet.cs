using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 18f;
    public float lifetime = 3f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude < 0.01f)
            rb.linearVelocity = Vector2.up * speed;

        Invoke(nameof(Kill), lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
    if (other.CompareTag("Enemy"))  // Make sure aliens are tagged "Enemy"
    {
        Alien alien = other.GetComponent<Alien>();
        if (alien != null)
            alien.TakeDamage(1);
        
        Destroy(gameObject);
    }
    }

    void OnDisable() => CancelInvoke();

    void Kill() => Destroy(gameObject);
}
