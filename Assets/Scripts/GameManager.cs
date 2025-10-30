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

    [Header("Rewards")]
    public int sessionShards = 0;
    public TMPro.TextMeshProUGUI shardsHudText; // optional in-game HUD
    public int missileUnlockThreshold = 25;

    const string TotalShardsKey = "TotalShards";
    const string MissilesUnlockedKey = "MissilesUnlocked";


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

    public void AddShard(int amount)
    {
        sessionShards += amount;
        UpdateShardsHUD();
    }

    void UpdateShardsHUD()
    {
        if (shardsHudText) shardsHudText.text = $"Shards: {sessionShards}";
    }

    void PersistShardsAndCheckUnlock()
    {
        int total = PlayerPrefs.GetInt(TotalShardsKey, 0) + sessionShards;
        PlayerPrefs.SetInt(TotalShardsKey, total);

        if (total >= missileUnlockThreshold)
            PlayerPrefs.SetInt(MissilesUnlockedKey, 1);

        PlayerPrefs.Save();
    }



    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");
        Player_Movement.Instance.enableInput(false);

        FreezeWorld();

        // 🔁 Replace restart UI with scene transition
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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

        PersistShardsAndCheckUnlock();

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
