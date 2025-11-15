using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Alien Type")]
    public AlienType alienType = AlienType.Normal;
    public enum AlienType { Normal, Rare }

    [Header("Movement - Zigzag")]
    public float forwardSpeed = 2f;
    public float zigzagAmplitude = 1.5f;  // How far left/right (reduced from 2)
    public float zigzagFrequency = 1.5f; // How fast it zigzags
    public float destroyY = -6f;
    public float screenPadding = 0.3f;   // Keep aliens this far from screen edges

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Shooting")]
    public GameObject alienBulletPrefab;
    public float shootInterval = 2f;      // Time between shots
    public float shootDelay = 1f;         // Initial delay before first shot
    public float bulletSpeed = 4f;
    private float nextShootTime;
    private Vector2 playerLastKnownPosition;

    [Header("Rewards - Rare Alien Only")]
    [Range(0f, 1f)]
    public float gooDropChance = 0.4f;    // 40% chance for rare aliens
    public GameObject gooPrefab;

    [Header("Explosion FX")]
    public GameObject explosionPrefab;
    public AudioClip explosionSound;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.1f;
    private Color originalColor;

    private float startX;
    private float elapsedTime = 0f;
    private AudioSource audioSource;
    private bool hasShot = false;
    private float screenCenterX;  // Use screen center for zigzag reference

    void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        
        // Get screen center X for consistent zigzag across full width
        Camera cam = Camera.main;
        if (cam)
        {
            screenCenterX = cam.transform.position.x;
        }
        else
        {
            screenCenterX = 0f;
        }
        
        // Schedule first shot
        nextShootTime = Time.time + shootDelay;
        
        // Get player's starting position
        UpdatePlayerPosition();

        // Setup visuals - FORCE correct rendering settings
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer)
        {
            originalColor = spriteRenderer.color;
            
            // CRITICAL: Ensure sprite is visible
            spriteRenderer.enabled = true;
            
            // Force sprite to render in front of background
            if (spriteRenderer.sortingLayerName == "Default" || spriteRenderer.sortingOrder < 5)
            {
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 10; // Higher than background
            }
            
            // Ensure alpha is not zero
            if (spriteRenderer.color.a < 0.1f)
            {
                Color fixedColor = spriteRenderer.color;
                fixedColor.a = 1f;
                spriteRenderer.color = fixedColor;
                originalColor = fixedColor;
            }
        }
        
        // Force Z position to be visible (in front of background, behind UI)
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
    }

    void Update()
    {
        if (GameManager_Level2.Instance != null && GameManager_Level2.Instance.isGameOver)
            return;

        MoveZigzag();
        HandleShooting();

        // Destroy if off screen
        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }

    void MoveZigzag()
    {
        elapsedTime += Time.deltaTime;

        // Move forward (down)
        float newY = transform.position.y - forwardSpeed * Time.deltaTime;

        // Zigzag motion centered on screen, not spawn position
        float zigzagOffset = Mathf.Sin(elapsedTime * zigzagFrequency * Mathf.PI) * zigzagAmplitude;
        float newX = screenCenterX + zigzagOffset;

        // Clamp to screen bounds
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            float camX = cam.transform.position.x;
            
            float minX = camX - halfWidth + screenPadding;
            float maxX = camX + halfWidth - screenPadding;
            
            newX = Mathf.Clamp(newX, minX, maxX);
        }

        // Keep Z at -1 to stay visible
        transform.position = new Vector3(newX, newY, -1f);
    }

    void HandleShooting()
    {
        if (Time.time >= nextShootTime && alienBulletPrefab)
        {
            UpdatePlayerPosition();
            ShootAtPlayer();
            nextShootTime = Time.time + shootInterval;
            hasShot = true;
        }
    }

    void UpdatePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLastKnownPosition = player.transform.position;
        }
    }

    void ShootAtPlayer()
    {
        if (!alienBulletPrefab) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, -0.5f, 0f);
        GameObject bullet = Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);

        AlienBullet alienBullet = bullet.GetComponent<AlienBullet>();
        if (alienBullet)
        {
            // Shoot at player's last known position
            alienBullet.Initialize(playerLastKnownPosition, bulletSpeed);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Visual feedback
        if (spriteRenderer)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        // Spawn explosion
        if (explosionPrefab)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            AudioSource explosionAudio = explosion.GetComponent<AudioSource>();
            if (explosionAudio && explosionSound)
                explosionAudio.PlayOneShot(explosionSound);
        }

        // Camera shake for all alien deaths
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.3f);
        }

        // Drop goo if rare alien
        if (alienType == AlienType.Rare && gooPrefab)
        {
            float roll = Random.value;
            if (roll <= gooDropChance)
            {
                SpawnGoo();
            }
        }

        Destroy(gameObject);
    }

    void SpawnGoo()
    {
        if (!gooPrefab) return;

        Vector3 spawnPos = transform.position;
        spawnPos.z = -1f; // Ensure it renders in front

        GameObject goo = Instantiate(gooPrefab, spawnPos, Quaternion.identity);
        
        Debug.Log($"[Alien] Rare alien dropped goo at {spawnPos}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager_Level2.Instance != null && GameManager_Level2.Instance.isGameOver)
            return;

        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            TakeDamage(1);
        }
        else if (other.GetComponent<Missile>() != null)
        {
            // Missile destroys in one hit (check by component instead of tag)
            TakeDamage(999);
            // Missile script will handle its own destruction
        }
        else if (other.CompareTag("Player"))
        {
            Player_Health player = other.GetComponent<Player_Health>();
            if (player != null)
                player.TakeDamage(1);

            Die();
        }
    }
}