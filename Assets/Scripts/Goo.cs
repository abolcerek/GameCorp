using UnityEngine;

public class Goo : MonoBehaviour
{
    [Header("Motion")]
    public float fallSpeed = 2f;
    public Vector2 burstVelocityRangeX = new Vector2(-1f, 1f);
    public Vector2 burstVelocityRangeY = new Vector2(0.5f, 2f);

    [Header("Value")]
    public int gooValue = 5;  // Worth more than regular shards
    
    [Header("Visual")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f;
            // Small initial burst for variety
            rb.linearVelocity = new Vector2(
                Random.Range(burstVelocityRangeX.x, burstVelocityRangeX.y),
                Random.Range(burstVelocityRangeY.x, burstVelocityRangeY.y)
            );
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Fall if no rigidbody
        if (!rb)
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Pulse effect for visual appeal
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;

        // Destroy if off-screen (below camera)
        if (transform.position.y < -6f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager_Level2.Instance != null)
        {
            // Add goo to Level 2's goo counter
            GameManager_Level2.Instance.AddGoo(gooValue);
            
            Debug.Log($"[Goo] Player collected goo worth {gooValue}!");
            
            // Optional: Play collection sound
            // AudioSource.PlayClipAtPoint(collectSound, transform.position);
            
            Destroy(gameObject);
        }
    }
}