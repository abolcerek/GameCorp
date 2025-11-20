using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    [Header("Player Sprites - Base")]
    public SpriteRenderer shipRenderer;
    public Sprite defaultShipSprite;
    public Sprite missileShipSprite;

    [Header("Player Sprites - With Shield")]
    public Sprite defaultShip_2Shields;
    public Sprite defaultShip_1Shield;
    public Sprite missileShip_2Shields;
    public Sprite missileShip_1Shield;

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
    
    private bool missilesUnlocked;
    private bool shieldUnlocked;

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("[Player_Movement] AWAKE CALLED!");
        Debug.Log($"[Player_Movement] GameObject: {gameObject.name}");
        Debug.Log($"[Player_Movement] Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log("========================================");
        
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Player_Movement] Instance SET successfully!");
        }
        else
        {
            Debug.LogError("[Player_Movement] DUPLICATE INSTANCE DETECTED! Destroying!");
            Destroy(gameObject);
            return;
        }
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[Player_Movement] NO RIGIDBODY2D FOUND!");
        }
        else
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("[Player_Movement] Rigidbody2D configured successfully");
        }
        
        if (!shipRenderer)
            shipRenderer = GetComponent<SpriteRenderer>();
            
        Debug.Log($"[Player_Movement] Awake complete. canMove = {canMove}");
    }

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[Player_Movement] START CALLED!");
        Debug.Log($"[Player_Movement] canMove = {canMove}");
        Debug.Log($"[Player_Movement] Instance is null? {Instance == null}");
        Debug.Log("========================================");
        
        missilesUnlocked = PlayerPrefs.GetInt("MissilesUnlocked", 0) == 1;
        shieldUnlocked = PlayerPrefs.GetInt("ShieldUnlocked", 0) == 1;

        UpdateSprite(2);
    }

    public void UpdateSprite(int shieldCount)
    {
        if (!shipRenderer) return;

        if (!shieldUnlocked)
            shieldCount = 0;

        Sprite newSprite = null;

        if (missilesUnlocked)
        {
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
        // SPAM DEBUG - Log every 60 frames (once per second at 60fps)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Player_Movement] Update running! Frame: {Time.frameCount}, canMove: {canMove}, TimeScale: {Time.timeScale}");
        }
        
        if (!canMove)
        {
            // Log this every time if canMove is false
            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[Player_Movement] canMove is FALSE! Input is disabled!");
            }
            return;
        }

        dx = 0f;
        dy = 0f;

        // Check if ANY input is happening
        if (Input.anyKey && Time.frameCount % 30 == 0)
        {
            Debug.Log("[Player_Movement] Some key is being pressed!");
        }

        if (Input.GetKey(rightKey))
        {
            Debug.Log("[Player_Movement] RIGHT key pressed!");
            dx += 1f;
        }
        if (Input.GetKey(leftKey))
        {
            Debug.Log("[Player_Movement] LEFT key pressed!");
            dx -= 1f;
        }
        if (Input.GetKey(upKey))
        {
            Debug.Log("[Player_Movement] UP key pressed!");
            dy += 1f;
        }
        if (Input.GetKey(downKey))
        {
            Debug.Log("[Player_Movement] DOWN key pressed!");
            dy -= 1f;
        }
        
        if (dx != 0 || dy != 0)
        {
            Debug.Log($"[Player_Movement] Movement calculated! dx={dx}, dy={dy}");
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector2 dir = new Vector2(dx, dy);
        rb.linearVelocity = dir * moveSpeed;
        
        if (dir.magnitude > 0 && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[Player_Movement] FixedUpdate applying velocity: {rb.linearVelocity}");
        }
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -2.5f, 2.5f);
        pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
        transform.position = pos;
    }

    public void enableInput(bool flag)
    {
        Debug.Log("========================================");
        Debug.Log($"[Player_Movement] enableInput({flag}) CALLED!");
        Debug.Log($"[Player_Movement] Previous canMove: {canMove}");
        Debug.Log($"[Player_Movement] Called from: {new System.Diagnostics.StackTrace()}");
        Debug.Log("========================================");
        
        canMove = flag;
        if (!flag && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"[Player_Movement] New canMove: {canMove}");
    }
}