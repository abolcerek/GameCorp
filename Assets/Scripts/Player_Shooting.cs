using UnityEngine;

public class Player_Shooting : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;
    public float fireCooldown = 0.12f;

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float missileSpeed = 8f;
    public float missileCooldown = 0.7f;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    private float nextFireAt = 0f;

    private enum AmmoType { Laser, Missile }
    private AmmoType currentAmmo = AmmoType.Laser;

    bool missilesUnlocked;
    const string MissilesUnlockedKey = "MissilesUnlocked";


    void Start()
    {
        // Create one reusable AudioSource instead of adding one per shot
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.7f;
        missilesUnlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1;
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleShooting();
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentAmmo = AmmoType.Laser;
            Debug.Log("Switched to Laser");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (missilesUnlocked)
            {
                currentAmmo = AmmoType.Missile;
                Debug.Log("Switched to Missile");
            }
            else
            {
                Debug.Log("Missiles locked: collect more shards!");
                // optional: play a "locked" SFX/UI flash
            }
        }

    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireAt)
        {
            switch (currentAmmo)
            {
                case AmmoType.Laser:
                    Fire(bulletPrefab, bulletSpeed);
                    nextFireAt = Time.time + fireCooldown;
                    break;

                case AmmoType.Missile:
                    Fire(missilePrefab, missileSpeed);
                    nextFireAt = Time.time + missileCooldown;
                    break;
            }
        }
    }

    void Fire(GameObject prefab, float speed)
    {
        if (shootSound) audioSource.PlayOneShot(shootSound);

        Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f);
        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

        var rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * speed;
        rb.gravityScale = 0f;
    }
}
