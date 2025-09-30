using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int startingLives = 3;
    public TextMeshProUGUI livesText;

    private int lives;

    void Start()
    {
        lives = startingLives;
        UpdateUI();
    }

    public void SetLives(int value)
    {
        lives = Mathf.Max(0, value);
        UpdateUI();
    }

    public void LoseLife(int amount = 1)
    {
        SetLives(lives - amount);
        // Need to handle game over when lives = 0
    }

    public int CurrentLives() => lives;

    private void UpdateUI()
    {
        if (livesText != null)  {
            livesText.text = $"Lives: {lives}";
        }
}
}
