using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(0, 6, -1);
    public float spawnDelay = 2f;

    [Header("State")]
    public bool bossSpawned = false;
    private GameObject currentBoss;

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

        currentBoss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        bossSpawned = true;

        Debug.Log($"[BossSpawner] Boss spawned at {bossSpawnPosition}");

        // Optional: Play boss music, show health bar, etc.
        // TODO: Add boss intro sequence
    }

    /// <summary>
    /// Check if boss is defeated
    /// </summary>
    public bool IsBossDefeated()
    {
        return bossSpawned && currentBoss == null;
    }
}