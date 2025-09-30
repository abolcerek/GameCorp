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

    void OnDisable() => CancelInvoke();

    void Kill() => Destroy(gameObject);
}
