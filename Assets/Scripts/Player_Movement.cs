using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    public string leftKey = "left";
    public string rightKey = "right";
    public string upKey = "up";
    public string downKey = "down";
    public bool canMove = true;
    public static Player_Movement Instance;

    public float moveSpeed = 7f;

    Rigidbody2D rb;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
    }

    void Update()
    {
        if (!canMove) {
            return;
        }

        float dx = 0f;
        float dy = 0f;

        if (Input.GetKey(rightKey)) {
            dx += 1f;
        }
        if (Input.GetKey(leftKey))  {dx -= 1f;
        }
        if (Input.GetKey(upKey))    {dy += 1f;
        }
        if (Input.GetKey(downKey))  {dy -= 1f;
        }

        Vector2 dir = new Vector2(dx, dy);
        // if (dir.sqrMagnitude > 1f) dir.Normalize();

        rb.linearVelocity = dir * moveSpeed;
    }

    public void enableInput(bool flag)
    {
        canMove = flag;
        if (!flag && rb != null) {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
