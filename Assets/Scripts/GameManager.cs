using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int startingLives = 3;
    public GameObject gameOverText;
    public GameObject restartButton;
    public GameObject levelCompleteText;
    private int lives;
    public bool isGameOver = false;
    public static GameManager Instance;
    public Image[] lifeIcons; 

    [Header("Sounds")]
    public AudioClip lifeLostSound;
    private AudioSource audioSource;

    void Awake()
    {
    Instance = this;
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.volume = 0.5f;
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
        if (lifeLostSound) audioSource.PlayOneShot(lifeLostSound);
        SetLives(lives - amount);
        if (lives <= 0)
        {
            GameOver();
        }
    }

    public int CurrentLives() => lives;

    private void UpdateUI()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].enabled = (i < lives);
        }
    }


    private void GameOver()
    {
        // Implement game over logic here (e.g., show game over screen, restart level)
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");
        Player_Movement.Instance.enableInput(false);

        FreezeWorld();

        if (gameOverText != null) gameOverText.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOverByTimer()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Time's up! Level Complete!");
        Player_Movement.Instance.enableInput(false);

        FreezeWorld();

        if (levelCompleteText != null) levelCompleteText.SetActive(true);  // ← Show new text
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void FreezeWorld()
    {
    ScrollingBackground[] backgrounds = FindObjectsByType<ScrollingBackground>(FindObjectsSortMode.None);
    foreach (var bg in backgrounds)
    {
        bg.enabled = false;
    }

    FallingAsteroid[] asteroids = FindObjectsByType<FallingAsteroid>(FindObjectsSortMode.None);
    foreach (var a in asteroids)
    {
        Destroy(a.gameObject);
    }

    AsteroidSpawner spawner = FindFirstObjectByType<AsteroidSpawner>();
    if (spawner != null)
        spawner.enabled = false;
    }


}
