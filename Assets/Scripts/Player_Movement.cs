using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    [Header("Player Sprites")]
    public SpriteRenderer shipRenderer;
    public Sprite defaultShipSprite;
    public Sprite missileShipSprite;

    public KeyCode leftKey  = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode upKey    = KeyCode.UpArrow;
    public KeyCode downKey  = KeyCode.DownArrow;
    public bool canMove = true;
    public static Player_Movement Instance;

    public float moveSpeed = 7f;

    // ✅ Add these two variables so dx and dy exist everywhere
    private float dx;
    private float dy;

    private Rigidbody2D rb;

    void Start()
{
    // If you already have Start(), just add inside it
    SpriteRenderer sr = GetComponent<SpriteRenderer>();

    bool missilesUnlocked = PlayerPrefs.GetInt("MissilesUnlocked", 0) == 1;

    if (missilesUnlocked)
        sr.sprite = missileShipSprite;
    else
        sr.sprite = defaultShipSprite;
}


    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
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

    // ✅ Re-add the enableInput method so GameManager can disable movement
    public void enableInput(bool flag)
    {
        canMove = flag;
        if (!flag && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
