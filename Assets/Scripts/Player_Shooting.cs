using UnityEngine;

public class Player_Shooting : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;

    [Tooltip("Lasers per second (the script derives fireCooldown = 1 / shotsPerSecond).")]
    public float shotsPerSecond = 8f;
    [HideInInspector] public float fireCooldown = 0.12f;

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float missileSpeed = 8f;
    public float missileCooldown = 0.7f;

    [Header("Audio")]
    public AudioClip laserShootSound;
    public AudioClip missileShootSound;
    public AudioClip overheatSound;
    private AudioSource audioSource;

    [Header("Heat / Cooldown (Laser)")]
    [Tooltip("How much the meter increases per laser shot.")]
    public float heatPerShot = 6f;

    [Tooltip("When heat reaches this, the meter is full and lasers lock.")]
    public float maxHeat = 30f;

    [Tooltip("Cooling rate while NOT overheated (heat per second).")]
    public float heatCooldownPerSec = 10f;

    [Tooltip("Cooling rate while overheated (heat per second until it reaches 0).")]
    public float overheatCooldownPerSec = 14f;

    [Header("Stationary Penalty")]
    public float stationarySpeedThreshold = 0.05f;
    public float stationaryCooldownMultiplier = 1.6f;

    private float heat = 0f;
    private float nextFireAt = 0f;
    private bool overheated = false;

    private enum AmmoType { Laser, Missile }
    private AmmoType currentAmmo = AmmoType.Laser;

    bool missilesUnlocked;
    const string MissilesUnlockedKey = "MissilesUnlocked";

    private Rigidbody2D _rb;
    private Vector2 _lastPos;

    public float Heat       => heat;
    public float MaxHeat    => maxHeat;
    public float HeatPercent => Mathf.Clamp01(heat / Mathf.Max(0.0001f, maxHeat));
    public bool IsOverheated => overheated;

    public float SecondsUntilReady
    {
        get
        {
            float rate = overheated ? overheatCooldownPerSec : heatCooldownPerSec;
            return (rate <= 0f) ? 0f : heat / rate;
        }
    }

    void OnValidate()
    {
        fireCooldown = 1f / Mathf.Max(0.0001f, shotsPerSecond);
    }

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("[Player_Shooting] AWAKE CALLED!");
        Debug.Log("========================================");
        
        fireCooldown = 1f / Mathf.Max(0.0001f, shotsPerSecond);
    }

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[Player_Shooting] START CALLED!");
        Debug.Log($"[Player_Shooting] bulletPrefab assigned? {bulletPrefab != null}");
        Debug.Log($"[Player_Shooting] missilePrefab assigned? {missilePrefab != null}");
        Debug.Log("========================================");
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.7f;

        missilesUnlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1;
        Debug.Log($"[Player_Shooting] Missiles unlocked? {missilesUnlocked}");

        _rb = GetComponent<Rigidbody2D>();
        _lastPos = transform.position;
    }

    void Update()
    {
        // SPAM DEBUG - Log every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Player_Shooting] Update running! Frame: {Time.frameCount}, Overheated: {overheated}, Heat: {heat}/{maxHeat}");
        }
        
        HandleWeaponSwitch();
        HandleCooling();
        HandleShooting();
    }

    void HandleCooling()
    {
        float rate = overheated ? overheatCooldownPerSec : heatCooldownPerSec;

        if (heat > 0f && rate > 0f)
        {
            heat = Mathf.Max(0f, heat - rate * Time.deltaTime);
        }

        if (overheated && heat <= 0f)
        {
            heat = 0f;
            overheated = false;
            Debug.Log("[Player_Shooting] Cooled down! No longer overheated!");
        }
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentAmmo = AmmoType.Laser;
            nextFireAt = Mathf.Max(nextFireAt, Time.time + 0.1f);
            Debug.Log("[Player_Shooting] Switched to Laser");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (missilesUnlocked)
            {
                currentAmmo = AmmoType.Missile;
                nextFireAt = Mathf.Max(nextFireAt, Time.time + 0.1f);
                Debug.Log("[Player_Shooting] Switched to Missile");
            }
            else
            {
                Debug.Log("[Player_Shooting] Missiles locked: collect more shards!");
            }
        }
    }

    void HandleShooting()
    {
        if (overheated)
        {
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("[Player_Shooting] OVERHEATED! Cannot shoot!");
            }
            return;
        }

        // Check if space is pressed
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("[Player_Shooting] SPACE KEY PRESSED!");
            
            if (Time.time < nextFireAt)
            {
                Debug.Log($"[Player_Shooting] Cooldown active! Next fire: {nextFireAt}, Current: {Time.time}");
                return;
            }
        }

        if (!Input.GetKey(KeyCode.Space) || Time.time < nextFireAt)
            return;

        Debug.Log($"[Player_Shooting] FIRING! Ammo: {currentAmmo}, Prefab: {(currentAmmo == AmmoType.Laser ? bulletPrefab : missilePrefab)}");

        switch (currentAmmo)
        {
            case AmmoType.Laser:
            {
                Fire(bulletPrefab, bulletSpeed, laserShootSound);

                float cd = fireCooldown;
                if (IsStationary())
                    cd *= stationaryCooldownMultiplier;
                nextFireAt = Time.time + cd;

                heat += heatPerShot;

                if (heat >= maxHeat)
                {
                    heat = maxHeat;
                    overheated = true;
                    if (overheatSound) audioSource.PlayOneShot(overheatSound);
                    Debug.Log("[Player_Shooting] OVERHEATED!");
                }
                break;
            }

            case AmmoType.Missile:
            {
                Fire(missilePrefab, missileSpeed, missileShootSound);
                nextFireAt = Time.time + missileCooldown;
                break;
            }
        }
    }

    bool IsStationary()
    {
        float speed;
        if (_rb != null)
        {
            speed = _rb.linearVelocity.magnitude;
        }
        else
        {
            Vector2 cur = transform.position;
            speed = (cur - _lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPos = cur;
        }
        return speed < stationarySpeedThreshold;
    }

    void Fire(GameObject prefab, float speed, AudioClip sfx)
    {
        if (!prefab)
        {
            Debug.LogError("[Player_Shooting] NO PREFAB ASSIGNED! Cannot fire!");
            return;
        }

        Debug.Log($"[Player_Shooting] Fire() called! Prefab: {prefab.name}, Speed: {speed}");

        if (sfx) audioSource.PlayOneShot(sfx);

        Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f);
        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        Debug.Log($"[Player_Shooting] Projectile instantiated at: {spawnPos}");

        var rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * speed;
            rb.gravityScale = 0f;
            Debug.Log($"[Player_Shooting] Projectile velocity set to: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogError("[Player_Shooting] Projectile has no Rigidbody2D!");
        }
    }
}