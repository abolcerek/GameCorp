using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int startingLives = 3;
    public TextMeshProUGUI livesText;
    public GameObject gameOverText;
    public GameObject restartButton;
    private int lives;
    public bool isGameOver = false;
    public static GameManager Instance;

    void Awake()
    {
    Instance = this;
    }

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
        if (lives <= 0)
        {
            GameOver();
        }
    }

    public int CurrentLives() => lives;

    private void UpdateUI()
    {
        if (livesText != null)  {
            livesText.text = $"Lives: {lives}";
        }
    }

    private void GameOver()
    {
        // Implement game over logic here (e.g., show game over screen, restart level)
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");
        Player_Movement.Instance.enableInput(false);

        if (gameOverText != null) gameOverText.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
