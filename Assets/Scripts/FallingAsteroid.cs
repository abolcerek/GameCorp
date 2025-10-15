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
        // Instantiate explosion prefab
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position + new Vector3(0, 0, -1), Quaternion.identity);
            Debug.Log("Explosion triggered on " + gameObject.name);


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


}
