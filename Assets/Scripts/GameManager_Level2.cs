using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager_Level2 : MonoBehaviour
{
    public int startingLives = 3;
    public GameObject gameOverText;
    public GameObject restartButton;
    public GameObject levelCompleteText;
    private int lives;
    public bool isGameOver = false;
    public static GameManager_Level2 Instance;
    public Image[] lifeIcons; 

    [Header("Sounds")]
    public AudioClip lifeLostSound;
    private AudioSource audioSource;

    [Header("Level 2 Rewards - Goo")]
    public int sessionGoo = 0;  // Goo collected this session
    public TMPro.TextMeshProUGUI gooHudText;  // Display goo count in-game
    public int shieldUnlockThreshold = 15;  // Goo needed for shield

    [Header("Level 2 Timing")]
    public float levelDuration = 90f;  // How long Level 2 lasts

    // PlayerPrefs keys
    const string TotalGooKey = "TotalGoo";
    const string ShieldUnlockedKey = "ShieldUnlocked";

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
        UpdateGooHUD();
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

    public void AddGoo(int amount)
    {
        sessionGoo += amount;
        UpdateGooHUD();
        Debug.Log($"[GameManager_Level2] Goo collected! Session total: {sessionGoo}");
    }

    void UpdateGooHUD()
    {
        if (gooHudText) gooHudText.text = $"Goo: {sessionGoo}";
    }

    void PersistGooAndCheckUnlock()
    {
        // Save goo
        int totalGoo = PlayerPrefs.GetInt(TotalGooKey, 0) + sessionGoo;
        PlayerPrefs.SetInt(TotalGooKey, totalGoo);

        // Check shield unlock
        if (totalGoo >= shieldUnlockThreshold)
            PlayerPrefs.SetInt(ShieldUnlockedKey, 1);

        PlayerPrefs.Save();

        Debug.Log($"[GameManager_Level2] Progress saved! Total Goo: {totalGoo}, Shield Unlocked: {totalGoo >= shieldUnlockThreshold}");
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[GameManager_Level2] Game Over!");
        Player_Movement.Instance.enableInput(false);

        FreezeWorld();

        // Save goo progress
        PersistGooAndCheckUnlock();

        // Return to main menu
        SceneManager.LoadScene("MainMenu");
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

        Debug.Log("[GameManager_Level2] Time's up! Level Complete!");
        Player_Movement.Instance.enableInput(false);

        FreezeWorld();

        // Save goo progress
        PersistGooAndCheckUnlock();

        // Return to main menu after winning
        SceneManager.LoadScene("MainMenu");
    }

    public void FreezeWorld()
    {
        // Disable scrolling backgrounds
        ScrollingBackground[] backgrounds = FindObjectsByType<ScrollingBackground>(FindObjectsSortMode.None);
        foreach (var bg in backgrounds)
        {
            bg.enabled = false;
        }

        // Disable alien spawner
        AlienSpawner alienSpawner = FindFirstObjectByType<AlienSpawner>();
        if (alienSpawner != null)
            alienSpawner.enabled = false;

        // Destroy all remaining aliens
        Alien[] aliens = FindObjectsByType<Alien>(FindObjectsSortMode.None);
        foreach (var alien in aliens)
        {
            Destroy(alien.gameObject);
        }

        // Destroy alien bullets
        AlienBullet[] alienBullets = FindObjectsByType<AlienBullet>(FindObjectsSortMode.None);
        foreach (var bullet in alienBullets)
        {
            Destroy(bullet.gameObject);
        }

        // Destroy any uncollected goo
        Goo[] goos = FindObjectsByType<Goo>(FindObjectsSortMode.None);
        foreach (var goo in goos)
        {
            Destroy(goo.gameObject);
        }
    }
}