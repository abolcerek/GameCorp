using UnityEngine;

public class Player_Health : MonoBehaviour
{
    public GameManager GameManager;
    public int lives;

    void Start()
    {
        // Auto-find GameManager if not assigned in Inspector
        if (GameManager == null)
        {
            GameManager = GameManager.Instance;
        }

        if (GameManager == null)
        {
            Debug.LogError("[Player_Health] GameManager not found! Make sure GameManager exists in the scene.");
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (GameManager == null)
        {
            Debug.LogError("[Player_Health] GameManager is null! Cannot take damage.");
            return;
        }

        GameManager.LoseLife(amount);
        lives = GameManager.CurrentLives();
    }
}