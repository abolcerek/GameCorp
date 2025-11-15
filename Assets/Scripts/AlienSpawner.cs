using UnityEngine;

public class AlienSpawner : MonoBehaviour
{
    [Header("Alien Prefabs")]
    public GameObject normalAlien;  // Purple alien
    public GameObject rareAlien;    // Green alien

    [Header("Spawn Settings")]
    public float spawnRate = 3f;
    public float spawnHeightOffset = 1f;
    public float edgePadding = 0.1f;

    [Header("Timing")]
    public float gameDuration = 90f;

    private float timer = 0f;
    private float elapsed = 0f;

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        elapsed += Time.deltaTime;

        if (elapsed >= gameDuration)
        {
            GameManager.Instance.GameOverByTimer();
            return;
        }

        // Clamp timer so spawn rate changes take effect immediately
        if (timer > spawnRate)
            timer = spawnRate;

        if (timer >= spawnRate)
        {
            SpawnAlien();
            timer = 0f;
        }
    }

    void SpawnAlien()
    {
        // 80% normal, 20% rare
        float normalWeight = 0.8f;
        float rareWeight = 0.2f;
        float totalWeight = normalWeight + rareWeight;
        float roll = Random.value * totalWeight;

        GameObject prefabToSpawn = (roll < normalWeight) ? normalAlien : rareAlien;
        if (!prefabToSpawn) return;

        Camera cam = Camera.main;
        if (!cam || !cam.orthographic)
        {
            Debug.LogWarning("AlienSpawner: Main Camera missing or not orthographic.");
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float camX = cam.transform.position.x;
        float leftEdge = camX - halfWidth;
        float rightEdge = camX + halfWidth;

        float prefabHalfW = GetPrefabHalfWidth(prefabToSpawn);

        float minX = leftEdge + edgePadding + prefabHalfW;
        float maxX = rightEdge - edgePadding - prefabHalfW;

        if (minX > maxX) { float mid = (minX + maxX) * 0.5f; minX = maxX = mid; }

        float randomX = Random.Range(minX, maxX);
        float spawnY = cam.transform.position.y + halfHeight + spawnHeightOffset;

        Instantiate(prefabToSpawn, new Vector2(randomX, spawnY), Quaternion.identity);
    }

    float GetPrefabHalfWidth(GameObject prefab)
    {
        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return sr.bounds.extents.x;

        var col = prefab.GetComponentInChildren<Collider2D>();
        if (col != null) return col.bounds.extents.x;

        return 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (!cam || !cam.orthographic) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float yTop = cam.transform.position.y + halfH + spawnHeightOffset;
        float xL = cam.transform.position.x - halfW + edgePadding;
        float xR = cam.transform.position.x + halfW - edgePadding;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(xL, yTop, 0f), new Vector3(xR, yTop, 0f));
        Gizmos.DrawSphere(new Vector3(xL, yTop, 0f), 0.05f);
        Gizmos.DrawSphere(new Vector3(xR, yTop, 0f), 0.05f);
    }
}