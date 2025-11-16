using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(0, 6, -1);
    public float spawnDelay = 2f;

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
        Invoke(nameof(DoSpawnBoss), spawnDelay);
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
            if (healthBar)
            {
                healthBar.SetBoss(currentBoss.transform);
            }

            // Connect health bar to boss
            BossAlien bossAlien = currentBoss.GetComponent<BossAlien>();
            if (bossAlien && healthBar)
            {
                bossAlien.healthBar = healthBar;
            }

            Debug.Log("[BossSpawner] Boss health bar created and connected!");
        }
        else
        {
            Debug.LogWarning("[BossSpawner] No health bar prefab or canvas assigned!");
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