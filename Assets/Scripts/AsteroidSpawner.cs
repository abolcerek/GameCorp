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
    [Tooltip("Padding from the true screen edges to avoid spawning partially off-screen.")]
    public float edgePadding = 0.1f;

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
        // --- pick prefab by weights (same logic as before) ---
        float smallWeight = 0.7f;
        float mediumWeight = 0.25f;
        float largeWeight = 0.05f;

        if (elapsed < mediumAsteroidTime)
        {
            mediumWeight = 0f; largeWeight = 0f;
        }
        else if (elapsed < largeAsteroidTime)
        {
            largeWeight = 0f;
        }

        float totalWeight = smallWeight + mediumWeight + largeWeight;
        float roll = Random.value * totalWeight;

        GameObject prefabToSpawn = smallAsteroid;
        if (roll < smallWeight) prefabToSpawn = smallAsteroid;
        else if (roll < smallWeight + mediumWeight) prefabToSpawn = mediumAsteroid;
        else prefabToSpawn = largeAsteroid;

        if (!prefabToSpawn) return;

        // --- camera-based horizontal bounds ---
        Camera cam = Camera.main;
        if (!cam || !cam.orthographic)
        {
            Debug.LogWarning("AsteroidSpawner: Main Camera missing or not orthographic.");
            return;
        }

        // World-space half width of camera view
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Horizontal world edges at camera center
        float camX = cam.transform.position.x;
        float leftEdge = camX - halfWidth;
        float rightEdge = camX + halfWidth;

        // Prefab half-width in world units (use renderer/collider bounds if available)
        float prefabHalfW = GetPrefabHalfWidth(prefabToSpawn);

        // Final safe range
        float minX = leftEdge + edgePadding + prefabHalfW;
        float maxX = rightEdge - edgePadding - prefabHalfW;

        // Safety: if minX > maxX (giant prefab / tiny screen), collapse to center
        if (minX > maxX) { float mid = (minX + maxX) * 0.5f; minX = maxX = mid; }

        float randomX = Random.Range(minX, maxX);

        // Spawn just above the top of the camera view
        float spawnY = cam.transform.position.y + halfHeight + spawnHeightOffset;

        Instantiate(prefabToSpawn, new Vector2(randomX, spawnY), Quaternion.identity);
    }

    float GetPrefabHalfWidth(GameObject prefab)
    {
        // Try SpriteRenderer from the prefab
        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return sr.bounds.extents.x;

        // Try Collider2D
        var col = prefab.GetComponentInChildren<Collider2D>();
        if (col != null) return col.bounds.extents.x;

        // Fallback: assume ~0.5 world units if no size info
        return 0.5f;
    }

    // Visualize spawn band in Scene view
    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (!cam || !cam.orthographic) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float yTop = cam.transform.position.y + halfH + spawnHeightOffset;
        float xL = cam.transform.position.x - halfW + edgePadding;
        float xR = cam.transform.position.x + halfW - edgePadding;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(xL, yTop, 0f), new Vector3(xR, yTop, 0f));
        Gizmos.DrawSphere(new Vector3(xL, yTop, 0f), 0.05f);
        Gizmos.DrawSphere(new Vector3(xR, yTop, 0f), 0.05f);
    }
}
