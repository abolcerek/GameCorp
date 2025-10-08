using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public float spawnRate = 1.5f;
    public float spawnHeightOffset = 1f;

    private Camera mainCam;
    private float timer = 0f;

    void Start()
    {
        mainCam = Camera.main;

        timer = spawnRate;
        if (asteroidPrefab == null)
            Debug.LogError("Asteroid prefab is missing in the spawner!");
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnAsteroid();
            timer = 0f;
        }
    }

    void SpawnAsteroid()
    {
        if (asteroidPrefab == null)
        {
            Debug.LogError("Asteroid prefab is missing or destroyed!");
            return;
        }

        // Fixed X spawn range ±3.3
        float minX = -3.3f + asteroidPrefab.transform.localScale.x / 2f;
        float maxX = 3.3f - asteroidPrefab.transform.localScale.x / 2f;
        float randomX = Random.Range(minX, maxX);

        // Fixed Y spawn at top
        float spawnY = 5.2f + spawnHeightOffset; // always above top

        Vector2 spawnPos = new Vector2(randomX, spawnY);

        Debug.DrawLine(spawnPos, spawnPos + Vector2.up, Color.red, 5f);
        Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);

        Debug.Log("Spawning asteroid at X=" + randomX);
    }
}