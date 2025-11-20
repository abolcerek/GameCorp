using UnityEngine;
using System.Collections;

public class BossAlien : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float leftBound = -2f;
    public float rightBound = 2f;
    public float yPosition = 4f;  // Stay at top of screen
    private bool movingRight = true;

    [Header("Attack Patterns")]
    public GameObject alienBulletPrefab;
    public float bulletSpeed = 4f;
    
    [Header("Attack Pattern 1 - Aimed at Player")]
    public float aimedShotInterval = 2f;
    public int aimedShotCount = 3;  // Shoot 3 bullets at player
    private float nextAimedShotTime;

    [Header("Attack Pattern 2 - Spread Shot")]
    public float spreadShotInterval = 4f;
    public int spreadBulletCount = 8;  // 8-way spread
    private float nextSpreadShotTime;

    [Header("Attack Pattern 3 - Vertical Barrage")]
    public float barrageInterval = 3f;
    public int barrageColumns = 5;  // 5 vertical streams
    private float nextBarrageTime;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.1f;
    private Color originalColor;

    [Header("Death")]
    public GameObject explosionPrefab;
    public AudioClip explosionSound;
    public float deathDelay = 2f;

    [Header("Sounds")]
    public AudioClip hitSound;      // NEW: Boss hit sound (plays on damage)
    public AudioClip deathSound;    // Boss death sound

    [Header("UI")]
    public BossHealthBar healthBar;

    private Vector2 playerLastKnownPosition;
    private bool isDead = false;
    private AudioSource audioSource;  // Audio source for boss sounds

    void Start()
    {
        currentHealth = maxHealth;

        // Setup audio for death sound only (spawn sound plays in BossSpawner)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;

        // Setup sprite
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer)
            originalColor = spriteRenderer.color;

        // Position boss at top
        transform.position = new Vector3(0, yPosition, -1f);

        // Initialize timers with slight offsets so attacks don't overlap
        nextAimedShotTime = Time.time + 2f;
        nextSpreadShotTime = Time.time + 3f;
        nextBarrageTime = Time.time + 4f;

        // Setup health bar
        if (healthBar)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        // Camera shake on entrance
        if (BossShake.Instance)
            BossShake.Instance.Shake(0.5f, 0.5f);

        Debug.Log("[BossAlien] Boss spawned with " + maxHealth + " HP!");
    }

    void Update()
    {
        if (isDead) return;

        MoveLeftRight();
        UpdatePlayerPosition();
        HandleAttacks();
    }

    void MoveLeftRight()
    {
        // Move horizontally
        float direction = movingRight ? 1f : -1f;
        float newX = transform.position.x + (direction * moveSpeed * Time.deltaTime);

        // Check bounds and reverse
        if (newX >= rightBound)
        {
            newX = rightBound;
            movingRight = false;
        }
        else if (newX <= leftBound)
        {
            newX = leftBound;
            movingRight = true;
        }

        transform.position = new Vector3(newX, yPosition, -1f);
    }

    void UpdatePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLastKnownPosition = player.transform.position;
        }
    }

    void HandleAttacks()
    {
        // Attack Pattern 1: Aimed shots at player
        if (Time.time >= nextAimedShotTime)
        {
            StartCoroutine(AimedShotSequence());
            nextAimedShotTime = Time.time + aimedShotInterval;
        }

        // Attack Pattern 2: 360 degree spread
        if (Time.time >= nextSpreadShotTime)
        {
            FireSpreadShot();
            nextSpreadShotTime = Time.time + spreadShotInterval;
        }

        // Attack Pattern 3: Vertical barrage
        if (Time.time >= nextBarrageTime)
        {
            FireVerticalBarrage();
            nextBarrageTime = Time.time + barrageInterval;
        }
    }

    IEnumerator AimedShotSequence()
    {
        // Fire multiple shots at player with slight delay
        for (int i = 0; i < aimedShotCount; i++)
        {
            FireAimedShot();
            yield return new WaitForSeconds(0.3f);
        }
    }

    void FireAimedShot()
    {
        if (!alienBulletPrefab) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, -0.5f, 0f);
        GameObject bullet = Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);

        AlienBullet alienBullet = bullet.GetComponent<AlienBullet>();
        if (alienBullet)
        {
            alienBullet.Initialize(playerLastKnownPosition, bulletSpeed);
        }

        Debug.Log("[BossAlien] Fired aimed shot at player!");
    }

    void FireSpreadShot()
    {
        if (!alienBulletPrefab) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, -0.5f, 0f);

        // Fire bullets in all directions (360 degree spread)
        float angleStep = 360f / spreadBulletCount;
        
        for (int i = 0; i < spreadBulletCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            GameObject bullet = Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);
            
            // Set bullet velocity directly
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }

            // Rotate bullet sprite to face direction
            float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, rotAngle - 90);
        }

        Debug.Log("[BossAlien] Fired spread shot!");
    }

    void FireVerticalBarrage()
    {
        if (!alienBulletPrefab) return;

        // Fire vertical columns of bullets
        float startX = leftBound + 0.5f;
        float endX = rightBound - 0.5f;
        float spacing = (endX - startX) / (barrageColumns - 1);

        for (int i = 0; i < barrageColumns; i++)
        {
            float x = startX + (spacing * i);
            Vector3 spawnPos = new Vector3(x, transform.position.y - 0.5f, 0f);

            GameObject bullet = Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);
            
            // Fire straight down
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.down * bulletSpeed;
            }

            bullet.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        Debug.Log("[BossAlien] Fired vertical barrage!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Update health bar
        if (healthBar)
            healthBar.SetHealth(currentHealth);

        // Play hit sound
        if (hitSound && audioSource)
        {
            audioSource.PlayOneShot(hitSound);
            Debug.Log("[BossAlien] Playing hit sound!");
        }

        // Visual feedback
        StartCoroutine(FlashDamage());

        // Camera shake on hit
        if (BossShake.Instance)
            BossShake.Instance.Shake(0.15f, 0.2f);

        Debug.Log($"[BossAlien] Took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    IEnumerator FlashDamage()
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
        isDead = true;
        Debug.Log("[BossAlien] Boss defeated!");

        // Hide health bar
        if (healthBar)
            healthBar.gameObject.SetActive(false);

        // Stop all attacks
        StopAllCoroutines();

        // Big camera shake
        if (BossShake.Instance)
            BossShake.Instance.Shake(1f, 1f);

        // Play death sequence
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Play death sound
        if (deathSound && audioSource)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log("[BossAlien] Playing death sound!");
        }

        // Flash a few times
        for (int i = 0; i < 5; i++)
        {
            if (spriteRenderer)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn explosion
        if (explosionPrefab)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            AudioSource explosionAudio = explosion.GetComponent<AudioSource>();
            if (explosionAudio && explosionSound)
                explosionAudio.PlayOneShot(explosionSound);
        }

        // Notify game manager boss is dead
        if (GameManager_Level2.Instance)
        {
            GameManager_Level2.Instance.BossDefeated();
            Debug.Log("[BossAlien] Boss defeated! Notifying GameManager...");
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Bullets
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            TakeDamage(1);
        }
        // Missiles
        else if (other.GetComponent<Missile>() != null)
        {
            TakeDamage(10);  // Missiles do more damage to boss
        }
        // Player collision
        else if (other.CompareTag("Player"))
        {
            Player_Health player = other.GetComponent<Player_Health>();
            if (player != null)
                player.TakeDamage(1);
        }
    }
}