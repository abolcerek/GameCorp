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

    [Header("Background Music")]
    public AudioClip level1Music;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    private AudioSource musicSource;

    [Header("Rewards")]
    public int sessionShards = 0;
    public TMPro.TextMeshProUGUI shardsHudText;
    public int missileUnlockThreshold = 25;

    const string TotalShardsKey = "TotalShards";
    const string MissilesUnlockedKey = "MissilesUnlocked";
    const string Level2UnlockedKey = "Level2Unlocked";

    void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
    }

    void Start()
    {
        // FIX: Ensure time is running and player can move!
        Time.timeScale = 1f;
        
        lives = startingLives;
        UpdateUI();
        
        // FIX: Enable player input when level starts!
        if (Player_Movement.Instance)
        {
            Player_Movement.Instance.enableInput(true);
            Debug.Log("[GameManager] Player input ENABLED on start!");
        }

        // Play level music
        PlayLevelMusic();
    }

    void PlayLevelMusic()
    {
        if (level1Music == null) return;

        // Create separate AudioSource for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = level1Music;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();

        Debug.Log("[GameManager] Level 1 music playing");
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

        Debug.Log("[GameManager] Game Over! Going to AsteroidDeath scene...");
        
        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(false);

        FreezeWorld();
        PersistShardsAndCheckUnlock();
        
        // Go to death scene instead of directly to menu
        SceneManager.LoadScene("AsteroidDeath");
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
        
        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(false);

        FreezeWorld();
        PersistShardsAndCheckUnlock();
        
        // Unlock Level 2 when Level 1 is completed
        PlayerPrefs.SetInt(Level2UnlockedKey, 1);
        PlayerPrefs.Save();
        Debug.Log("[GameManager] Level 2 UNLOCKED!");
        
        // Go to transition scene instead of directly to menu
        SceneManager.LoadScene("TransitionScene");
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