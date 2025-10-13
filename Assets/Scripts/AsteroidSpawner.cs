using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Asteroid Prefabs")]
    public GameObject smallAsteroid;
    public GameObject mediumAsteroid;
    public GameObject largeAsteroid;

    [Header("Spawn Settings")]
    public float spawnRate = 1.5f;
    public float spawnHeightOffset = 1f;

    [Header("Timing")]
    public float gameDuration = 60f;
    public float mediumAsteroidTime = 15f; // unlock medium
    public float largeAsteroidTime = 30f;  // unlock large

    private float timer = 0f;
    private float elapsed = 0f;

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        elapsed += Time.deltaTime;

        // End game when time runs out
        if (elapsed >= gameDuration)
        {
            GameManager.Instance.GameOverByTimer();
            return;
        }

        // Spawn asteroids regularly
        if (timer >= spawnRate)
        {
            SpawnAsteroid();
            timer = 0f;
        }
    }

    void SpawnAsteroid()
    {
        // ✅ Define relative spawn weights
        float smallWeight = 0.7f;  // 70% chance
        float mediumWeight = 0.25f; // 25% chance
        float largeWeight = 0.05f;  // 5% chance

        // Adjust weights dynamically as game progresses
        if (elapsed < mediumAsteroidTime)
        {
            mediumWeight = 0f;
            largeWeight = 0f;
        }
        else if (elapsed < largeAsteroidTime)
        {
            largeWeight = 0f;
        }

        // Total weight of all available asteroid types
        float totalWeight = smallWeight + mediumWeight + largeWeight;
        float roll = Random.value * totalWeight;

        GameObject prefabToSpawn = smallAsteroid;

        if (roll < smallWeight)
            prefabToSpawn = smallAsteroid;
        else if (roll < smallWeight + mediumWeight)
            prefabToSpawn = mediumAsteroid;
        else
            prefabToSpawn = largeAsteroid;

        // Safety check
        if (prefabToSpawn == null) return;

        // Random horizontal spawn
        float minX = -3.3f + prefabToSpawn.transform.localScale.x / 2f;
        float maxX = 3.3f - prefabToSpawn.transform.localScale.x / 2f;
        float randomX = Random.Range(minX, maxX);

        float spawnY = 5.2f + spawnHeightOffset;
        Vector2 spawnPos = new Vector2(randomX, spawnY);

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

}
