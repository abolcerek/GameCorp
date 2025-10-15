using UnityEngine;

public class Player_Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;
    public float fireCooldown = 0.12f;
    float nextFireAt = 0f;
    public AudioClip shootSound;
    private AudioSource audioSource;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireAt)
        {
            Fire();
            nextFireAt = Time.time + fireCooldown;
        }
    }

    void Fire()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.7f;
        if (shootSound) audioSource.PlayOneShot(shootSound);
        Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f); // adjust Y as needed
        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        var rb = b.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * bulletSpeed;
        rb.gravityScale = 0f;
    }
}
