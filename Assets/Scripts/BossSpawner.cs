using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(0, 6, -1);
    public float spawnDelay = 2f;

    [Header("Sounds")]
    public AudioClip bossSpawnSound;  // NEW: Sound plays before boss spawns
    private AudioSource audioSource;

    [Header("Boss Music")]
    public AudioClip bossMusic;  // Music that plays during boss fight
    [Range(0f, 1f)]
    public float bossMusicVolume = 0.6f;
    private AudioSource musicSource;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;  // Assign BossHealthBar prefab
    public Canvas canvas;  // Reference to main Canvas

    [Header("State")]
    public bool bossSpawned = false;
    private GameObject currentBoss;
    private GameObject currentHealthBar;

    /// <summary>
    /// Called by LevelTransition when player chooses to fight boss
    /// </summary>
    public void SpawnBoss()
    {
        if (bossSpawned)
        {
            Debug.LogWarning("[BossSpawner] Boss already spawned!");
            return;
        }

        Debug.Log("[BossSpawner] Spawning boss...");

        // Setup audio source if not already created
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.8f;
        }

        // STOP LEVEL 2 MUSIC IMMEDIATELY (before boss entrance sound)
        if (GameManager_Level2.Instance != null)
        {
            GameManager_Level2.Instance.StopLevelMusic();
            Debug.Log("[BossSpawner] Level 2 music stopped for boss entrance");
        }

        // Play spawn sound (entrance music)
        if (bossSpawnSound && audioSource)
        {
            audioSource.PlayOneShot(bossSpawnSound);
            Debug.Log("[BossSpawner] Playing boss entrance sound...");
            
            // Calculate delay: use sound length or default delay, whichever is longer
            float soundLength = bossSpawnSound.length;
            float totalDelay = Mathf.Max(spawnDelay, soundLength);
            
            Debug.Log($"[BossSpawner] Boss will appear in {totalDelay} seconds (after entrance sound)");
            Invoke(nameof(DoSpawnBoss), totalDelay);
        }
        else
        {
            // No sound, use default delay
            Invoke(nameof(DoSpawnBoss), spawnDelay);
        }
    }

    void DoSpawnBoss()
    {
        if (!bossPrefab)
        {
            Debug.LogError("[BossSpawner] No boss prefab assigned!");
            return;
        }

        // Spawn boss
        currentBoss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        bossSpawned = true;

        Debug.Log($"[BossSpawner] Boss spawned at {bossSpawnPosition}");

        // Create health bar
        if (healthBarPrefab && canvas)
        {
            currentHealthBar = Instantiate(healthBarPrefab, canvas.transform);
            
            BossHealthBar healthBar = currentHealthBar.GetComponent<BossHealthBar>();
            
            // Connect health bar to boss
            BossAlien bossAlien = currentBoss.GetComponent<BossAlien>();
            if (bossAlien && healthBar)
            {
                bossAlien.healthBar = healthBar;
                Debug.Log("[BossSpawner] Boss health bar created and connected!");
                
                // Trigger screech and music sequence
                StartCoroutine(BossIntroSequence(bossAlien));
            }
            else
            {
                Debug.LogWarning("[BossSpawner] Could not connect health bar to boss!");
            }
        }
        else
        {
            Debug.LogWarning("[BossSpawner] No health bar prefab or canvas assigned!");
        }
    }
    
    System.Collections.IEnumerator BossIntroSequence(BossAlien boss)
    {
        // Wait a moment for boss to be visible
        yield return new WaitForSeconds(0.5f);
        
        // Boss screeches (plays hit sound)
        if (boss.hitSound && audioSource)
        {
            audioSource.PlayOneShot(boss.hitSound);
            Debug.Log("[BossSpawner] Boss screeched!");
        }
        
        // Wait for screech to finish (or a minimum time)
        float screechDelay = boss.hitSound != null ? boss.hitSound.length : 1f;
        yield return new WaitForSeconds(screechDelay);
        
        // Start boss music
        StartBossMusic();
    }
    
    void StartBossMusic()
    {
        if (bossMusic == null)
        {
            Debug.LogWarning("[BossSpawner] No boss music assigned!");
            return;
        }
        
        // Level 2 music already stopped when entrance sound started
        // Now start boss battle music
        
        // Create music source if needed
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        musicSource.clip = bossMusic;
        musicSource.volume = bossMusicVolume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();
        
        Debug.Log("[BossSpawner] Boss battle music started!");
    }
    
    public void StopBossMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[BossSpawner] Boss music stopped!");
        }
    }

    /// <summary>
    /// Check if boss is defeated
    /// </summary>
    public bool IsBossDefeated()
    {
        return bossSpawned && currentBoss == null;
    }
}