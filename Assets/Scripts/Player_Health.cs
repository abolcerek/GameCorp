using UnityEngine;

public class Player_Health : MonoBehaviour
{
    public int lives;

    void Start()
    {
        // Lives will be synced from whichever GameManager is active
    }

    public void TakeDamage(int amount = 1)
    {
        // Try Level 1 GameManager first
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(amount);
            lives = GameManager.Instance.CurrentLives();
            return;
        }

        // Try Level 2 GameManager
        if (GameManager_Level2.Instance != null)
        {
            GameManager_Level2.Instance.LoseLife(amount);
            lives = GameManager_Level2.Instance.CurrentLives();
            return;
        }

        Debug.LogError("[Player_Health] No GameManager found! Cannot take damage.");
    }
}