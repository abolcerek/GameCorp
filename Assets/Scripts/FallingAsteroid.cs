using UnityEngine;

public class FallingAsteroid : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 5f;
    public float destroyY = -6f;

    [Header("Asteroid Stats")]
    public int health = 1;

    [Header("Asteroid Prefabs")]
    public GameObject mediumAsteroidPrefab; // used by large asteroids
    public GameObject smallAsteroidPrefab;  // used by medium asteroids

    [Header("Explosion FX")]
    public GameObject explosionPrefab;  // assign glowing explosion prefab
    public AudioClip smallExplosionSound;
    public AudioClip mediumExplosionSound;
    public AudioClip largeExplosionSound;

    [Header("Rewards")]
    public GameObject shardPrefab;
    public int shardsMin = 3;
    public int shardsMax = 6;
    public bool dropsShards = false; // enable on Large Asteroids

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
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

            Explode();
            Destroy(gameObject);
        }
    }

    void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            // Spawn smaller asteroids if applicable
            if (mediumAsteroidPrefab != null && gameObject.name.Contains("Large"))
                SpawnChildAsteroids(mediumAsteroidPrefab);
            else if (smallAsteroidPrefab != null && gameObject.name.Contains("Medium"))
                SpawnChildAsteroids(smallAsteroidPrefab);

            Explode();
            Destroy(gameObject);
        }
    }

    void SpawnChildAsteroids(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos1 = transform.position + new Vector3(-0.5f, 0.3f, 0f);
        Vector3 pos2 = transform.position + new Vector3(0.5f, 0.3f, 0f);

        GameObject obj1 = Instantiate(prefab, pos1, Quaternion.identity);
        GameObject obj2 = Instantiate(prefab, pos2, Quaternion.identity);

        Rigidbody2D rb1 = obj1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = obj2.GetComponent<Rigidbody2D>();
        if (rb1) rb1.linearVelocity = new Vector2(-1f, -3f);
        if (rb2) rb2.linearVelocity = new Vector2(1f, -3f);
    }

    void Explode()
    {
        if (dropsShards)
        {
            Debug.Log($"[Asteroid] {name} exploding @ {transform.position} → spawning shards");
            SpawnShards();
        }

        // Instantiate explosion prefab
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position + new Vector3(0, 0, -1), Quaternion.identity);

            // Optionally choose which sound the prefab plays
            AudioSource explosionAudio = explosion.GetComponent<AudioSource>();
            if (explosionAudio != null)
            {
                if (gameObject.name.Contains("Large") && largeExplosionSound)
                    explosionAudio.PlayOneShot(largeExplosionSound);
                else if (gameObject.name.Contains("Medium") && mediumExplosionSound)
                    explosionAudio.PlayOneShot(mediumExplosionSound);
                else if (gameObject.name.Contains("Small") && smallExplosionSound)
                    explosionAudio.PlayOneShot(smallExplosionSound);
            }
        }

        // ✅ Camera shake only for large asteroids
        if (CameraShake.Instance != null && gameObject.name.Contains("Large"))
        {
            CameraShake.Instance.Shake(0.35f, 0.5f);
        }
    }

    void SpawnShards()
    {
        if (shardPrefab == null) { Debug.LogWarning("[Shards] shardPrefab is NULL"); return; }

        int count  = Random.Range(shardsMin, shardsMax + 1);
        float radius = 1.0f;                 // keep tight so they stay on-screen
        Vector3 origin = transform.position;

        Camera cam = Camera.main;

        for (int i = 0; i < count; i++)
        {
            // radial scatter, slightly biased downward so they don't pop above camera
            float angle    = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(radius * 0.4f, radius);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance,
                                        Mathf.Sin(angle) * distance - 0.25f,
                                        0f);

            Vector3 spawnPos = origin + offset;

            // clamp into the camera’s visible horizontal range (safety)
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(spawnPos);
                // if somehow outside, nudge inward a bit
                if (vp.x < 0.05f) spawnPos.x = cam.ViewportToWorldPoint(new Vector3(0.05f, vp.y, vp.z)).x;
                if (vp.x > 0.95f) spawnPos.x = cam.ViewportToWorldPoint(new Vector3(0.95f, vp.y, vp.z)).x;
                if (vp.y < 0.05f) spawnPos.y = cam.ViewportToWorldPoint(new Vector3(vp.x, 0.05f, vp.z)).y;
                if (vp.y > 0.95f) spawnPos.y = cam.ViewportToWorldPoint(new Vector3(vp.x, 0.95f, vp.z)).y;
            }

            // ensure it renders in front
            spawnPos.z = -1f;

            GameObject shard = Instantiate(shardPrefab, spawnPos, Quaternion.identity);

            // force renderer to a visible layer/order (even if prefab was mis-set)
            var sr = shard.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Use a sorting layer that is above your background; if you don't have one,
                // leave this line or set sr.sortingLayerName = "Default";
                sr.sortingLayerName = "Foreground";   // create this sorting layer once in Project Settings
                sr.sortingOrder = 20;                 // bigger than background order
                // do NOT change color alpha here; keep prefab’s look
            }

            // keep prefab’s scale (commented out any random scale that might shrink it)
            // float randomScale = Random.Range(0.8f, 1.2f);
            // shard.transform.localScale = Vector3.one * randomScale;

            // small outward drift so they separate
            var rb = shard.GetComponent<Rigidbody2D>();
            if (rb)
            {
                Vector2 burst = offset.normalized * Random.Range(1.3f, 2.3f);
                rb.gravityScale = 0f;
                rb.linearVelocity = burst;
            }

            // one-line proof per shard with key render info
            Debug.Log($"[Shards] #{i+1}/{count} at {spawnPos}  layer={sr?.sortingLayerName}/{sr?.sortingOrder}  scale={shard.transform.localScale}");
        }

        Debug.Log($"[Shards] DONE: spawned {count} at origin {origin}");
    }
}
