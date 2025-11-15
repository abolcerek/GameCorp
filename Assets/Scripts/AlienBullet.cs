using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    public float lifetime = 5f;
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;
    private bool hasHit = false;  // Prevent multiple hits

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void Initialize(Vector2 targetPosition, float bulletSpeed)
    {
        speed = bulletSpeed;
        
        // Calculate direction to target
        direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Apply velocity
        if (rb)
            rb.linearVelocity = direction * speed;
        
        // Rotate bullet to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 to align sprite

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;  // Already hit something, ignore
        
        if (other.CompareTag("Player"))
        {
            hasHit = true;  // Mark as hit
            
            Player_Health player = other.GetComponent<Player_Health>();
            if (player != null)
                player.TakeDamage(1);
            
            Destroy(gameObject);
        }
        // Ignore collisions with aliens and other bullets
        // Only destroy on player hit
    }
}