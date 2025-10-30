using UnityEngine;

public class Player_Shooting : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;

    [Tooltip("Lasers per second (the script derives fireCooldown = 1 / shotsPerSecond).")]
    public float shotsPerSecond = 8f; // e.g., 8 shots/s
    [HideInInspector] public float fireCooldown = 0.12f; // auto-updated from shotsPerSecond

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float missileSpeed = 8f;
    public float missileCooldown = 0.7f;

    [Header("Audio")]
    public AudioClip laserShootSound;   // laser SFX
    public AudioClip missileShootSound; // missile SFX
    public AudioClip overheatSound;     // overheat SFX
    private AudioSource audioSource;

    // ===== Classic Cooldown (Laser) =====
    [Header("Heat / Cooldown (Laser)")]
    [Tooltip("How much the meter increases per laser shot.")]
    public float heatPerShot = 6f;

    [Tooltip("When heat reaches this, the meter is full and lasers lock.")]
    public float maxHeat = 30f;

    [Tooltip("Cooling rate while NOT overheated (heat per second).")]
    public float heatCooldownPerSec = 10f;

    [Tooltip("Cooling rate while overheated (heat per second until it reaches 0).")]
    public float overheatCooldownPerSec = 14f;

    // ===== Stationary penalty (encourage movement) =====
    [Header("Stationary Penalty")]
    public float stationarySpeedThreshold = 0.05f; // below this = stationary
    public float stationaryCooldownMultiplier = 1.6f; // slower ROF when parked

    // runtime state
    private float heat = 0f;     // 0..maxHeat
    private float nextFireAt = 0f;
    private bool overheated = false; // classic lockout while bar drains to 0

    private enum AmmoType { Laser, Missile }
    private AmmoType currentAmmo = AmmoType.Laser;

    bool missilesUnlocked;
    const string MissilesUnlockedKey = "MissilesUnlocked";

    // movement sampling
    private Rigidbody2D _rb;
    private Vector2 _lastPos;

    // ---- Public readouts for a UI bar ----
    public float Heat       => heat;
    public float MaxHeat    => maxHeat;
    public float HeatPercent => Mathf.Clamp01(heat / Mathf.Max(0.0001f, maxHeat));
    public bool IsOverheated => overheated;

    // Approx seconds until ready (for label) based on current cooling mode
    public float SecondsUntilReady
    {
        get
        {
            float rate = overheated ? overheatCooldownPerSec : heatCooldownPerSec;
            return (rate <= 0f) ? 0f : heat / rate;
        }
    }

    // Keep fireCooldown in sync with shotsPerSecond in editor and at runtime
    void OnValidate()
    {
        fireCooldown = 1f / Mathf.Max(0.0001f, shotsPerSecond);
    }

    void Awake()
    {
        fireCooldown = 1f / Mathf.Max(0.0001f, shotsPerSecond);
    }

    void Start()
    {
        // Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.7f;

        // Progress
        missilesUnlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1;

        // Movement
        _rb = GetComponent<Rigidbody2D>();
        _lastPos = transform.position;
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleCooling();   // classic cooldown cooling path
        HandleShooting();
    }

    void HandleCooling()
    {
        // Classic behavior:
        //  - If overheated, force-cool to 0 (bar drains, no firing).
        //  - Else, passively cool toward 0.
        float rate = overheated ? overheatCooldownPerSec : heatCooldownPerSec;

        if (heat > 0f && rate > 0f)
        {
            heat = Mathf.Max(0f, heat - rate * Time.deltaTime);
        }

        // When overheated and we've cooled to 0, unlock firing.
        if (overheated && heat <= 0f)
        {
            heat = 0f;
            overheated = false;
        }
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentAmmo = AmmoType.Laser;
            nextFireAt = Mathf.Max(nextFireAt, Time.time + 0.1f); // debounce
            Debug.Log("Switched to Laser");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (missilesUnlocked)
            {
                currentAmmo = AmmoType.Missile;
                nextFireAt = Mathf.Max(nextFireAt, Time.time + 0.1f); // debounce
                Debug.Log("Switched to Missile");
            }
            else
            {
                Debug.Log("Missiles locked: collect more shards!");
            }
        }
    }

    void HandleShooting()
    {
        // If overheated, classic rule: cannot shoot until bar fully cools to 0.
        if (overheated) return;

        // Regular cooldown gate
        if (!Input.GetKey(KeyCode.Space) || Time.time < nextFireAt)
            return;

        switch (currentAmmo)
        {
            case AmmoType.Laser:
            {
                Fire(bulletPrefab, bulletSpeed, laserShootSound);

                // Fire rate = 1 / shotsPerSecond, with stationary penalty if parked
                float cd = fireCooldown;
                if (IsStationary())
                    cd *= stationaryCooldownMultiplier;
                nextFireAt = Time.time + cd;

                // Add heat for this shot
                heat += heatPerShot;

                // If the shot filled the meter, clamp and lock out
                if (heat >= maxHeat)
                {
                    heat = maxHeat;
                    overheated = true; // enter lockout until cooled to 0
                    if (overheatSound) audioSource.PlayOneShot(overheatSound);
                }
                break;
            }

            case AmmoType.Missile:
            {
                Fire(missilePrefab, missileSpeed, missileShootSound);
                nextFireAt = Time.time + missileCooldown;
                // Missiles do not add heat (tactical)
                break;
            }
        }
    }

    bool IsStationary()
    {
        float speed;
        if (_rb != null)
        {
            // If your version doesn’t have linearVelocity, switch to velocity
            speed = _rb.linearVelocity.magnitude;
            // speed = _rb.velocity.magnitude;
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
        if (!prefab) return;

        if (sfx) audioSource.PlayOneShot(sfx);

        Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f);
        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

        var rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * speed; // or rb.velocity if needed
            rb.gravityScale = 0f;
        }
    }
}
