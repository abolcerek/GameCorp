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

        bool hitSomething = false;

        // Check for boss alien
        var bossAlien = other.GetComponent<BossAlien>();
        if (bossAlien != null)
        {
            bossAlien.TakeDamage(10); // Missiles do 10 damage to boss (not 999)
            hitSomething = true;
        }

        // Check for regular alien
        var alien = other.GetComponent<Alien>();
        if (alien != null)
        {
            alien.TakeDamage(damage); // 999 damage = instant kill
            hitSomething = true;
        }

        // Check for asteroid
        var asteroid = other.GetComponentInParent<FallingAsteroid>();
        if (asteroid != null)
        {
            asteroid.TakeDamage(damage);
            hitSomething = true;
        }

        // If we didn't hit anything valid, don't detonate
        if (!hitSomething)
            return;

        // Spawn impact VFX at contact point
        if (hitExplosionPrefab)
            Instantiate(hitExplosionPrefab, hitPoint, Quaternion.identity);

        detonated = true;
        Destroy(gameObject);
    }
}