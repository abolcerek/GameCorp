using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Missile")]
    public float speed = 8f;
    public float lifetime = 4f;
    public int damage = 999;                 // big damage

    [Header("VFX/SFX")]
    public GameObject hitExplosionPrefab;    // assign in Inspector

    private Rigidbody2D rb;
    private bool detonated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void OnEnable()
    {
        if (rb) rb.linearVelocity = Vector2.up * speed;   // or rb.velocity
        Invoke(nameof(Kill), lifetime);
    }

    void OnDisable() => CancelInvoke();
    void Kill() { if (!detonated) Destroy(gameObject); }

    // Support either trigger or collision
    void OnTriggerEnter2D(Collider2D other)  { HandleHit(other.gameObject, other.ClosestPoint(transform.position)); }
    void OnCollisionEnter2D(Collision2D col) { HandleHit(col.collider.gameObject, col.GetContact(0).point); }

    void HandleHit(GameObject other, Vector2 hitPoint)
    {
        if (detonated) return;

        // Try to find the asteroid script
        var asteroid = other.GetComponentInParent<FallingAsteroid>();

        if (asteroid != null)
        {
            // Prefer ApplyDamage if you added the wrapper; else TakeDamage works too
            asteroid.TakeDamage(damage);   // or asteroid.ApplyDamage(damage);
        }
        else if (!other.CompareTag("Asteroid"))
        {
            // Not an asteroid → ignore (or detonate anyway if you want)
            return;
        }
        else
        {
            // Fallback: tagged Asteroid with no script
            Destroy(other);
        }

        // Spawn impact VFX at contact point
        if (hitExplosionPrefab)
            Instantiate(hitExplosionPrefab, hitPoint, Quaternion.identity);

        detonated = true;
        Destroy(gameObject);
    }
}
