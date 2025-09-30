using UnityEngine;

public class Player_Health : MonoBehaviour
{
    public GameManager GameManager;
    public int lives; 

    public void TakeDamage(int amount = 1)
    {
        GameManager.LoseLife(amount);
        lives = GameManager.CurrentLives();
    }
}
