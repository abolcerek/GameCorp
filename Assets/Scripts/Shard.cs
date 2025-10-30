using UnityEngine;

public class Shard : MonoBehaviour
{
    [Header("Motion")]
    public float fallSpeed = 1.5f;
    public Vector2 burstVelocityRangeX = new Vector2(-1.5f, 1.5f);
    public Vector2 burstVelocityRangeY = new Vector2(1.0f, 3.0f);

    [Header("Value")]
    public int value = 1;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(
                Random.Range(burstVelocityRangeX.x, burstVelocityRangeX.y),
                Random.Range(burstVelocityRangeY.x, burstVelocityRangeY.y)
            );
        }
    }

    void Update()
    {
        if (!rb)
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.AddShard(value);
            Destroy(gameObject);
        }
    }
}
