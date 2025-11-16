using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    [Header("Player Sprites - Base")]
    public SpriteRenderer shipRenderer;
    public Sprite defaultShipSprite;       // No missiles, no shield
    public Sprite missileShipSprite;       // Missiles unlocked, no shield

    [Header("Player Sprites - With Shield")]
    public Sprite defaultShip_2Shields;    // No missiles, 2 shields
    public Sprite defaultShip_1Shield;     // No missiles, 1 shield
    public Sprite missileShip_2Shields;    // Missiles, 2 shields
    public Sprite missileShip_1Shield;     // Missiles, 1 shield

    [Header("Movement")]
    public KeyCode leftKey  = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode upKey    = KeyCode.UpArrow;
    public KeyCode downKey  = KeyCode.DownArrow;
    public bool canMove = true;
    public float moveSpeed = 7f;

    public static Player_Movement Instance;

    private float dx;
    private float dy;
    private Rigidbody2D rb;
    
    // Shield state tracking
    private bool missilesUnlocked;
    private bool shieldUnlocked;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (!shipRenderer)
            shipRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Check what's unlocked
        missilesUnlocked = PlayerPrefs.GetInt("MissilesUnlocked", 0) == 1;
        shieldUnlocked = PlayerPrefs.GetInt("ShieldUnlocked", 0) == 1;

        // Set initial sprite based on unlocks
        UpdateSprite(2); // Start with full shields if unlocked
    }

    /// <summary>
    /// Updates the player sprite based on missiles unlocked and shield count
    /// </summary>
    /// <param name="shieldCount">0 = no shield, 1 = one shield, 2 = two shields</param>
    public void UpdateSprite(int shieldCount)
    {
        if (!shipRenderer) return;

        // If shields aren't unlocked, ignore shield count
        if (!shieldUnlocked)
            shieldCount = 0;

        // Select the appropriate sprite
        Sprite newSprite = null;

        if (missilesUnlocked)
        {
            // Missile variants
            switch (shieldCount)
            {
                case 2:
                    newSprite = missileShip_2Shields;
                    break;
                case 1:
                    newSprite = missileShip_1Shield;
                    break;
                default:
                    newSprite = missileShipSprite;
                    break;
            }
        }
        else
        {
            // Default variants
            switch (shieldCount)
            {
                case 2:
                    newSprite = defaultShip_2Shields;
                    break;
                case 1:
                    newSprite = defaultShip_1Shield;
                    break;
                default:
                    newSprite = defaultShipSprite;
                    break;
            }
        }

        // Apply sprite if found
        if (newSprite != null)
        {
            shipRenderer.sprite = newSprite;
            Debug.Log($"[Player_Movement] Sprite updated: Shields={shieldCount}, Missiles={missilesUnlocked}");
        }
        else
        {
            Debug.LogWarning($"[Player_Movement] Missing sprite for: Shields={shieldCount}, Missiles={missilesUnlocked}");
        }
    }

    void Update()
    {
        if (!canMove) return;

        dx = 0f;
        dy = 0f;

        if (Input.GetKey(rightKey)) dx += 1f;
        if (Input.GetKey(leftKey))  dx -= 1f;
        if (Input.GetKey(upKey))    dy += 1f;
        if (Input.GetKey(downKey))  dy -= 1f;
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector2 dir = new Vector2(dx, dy);
        rb.linearVelocity = dir * moveSpeed;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -2.5f, 2.5f);
        pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
        transform.position = pos;
    }

    public void enableInput(bool flag)
    {
        canMove = flag;
        if (!flag && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}