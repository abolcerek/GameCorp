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
    public bool isBossFight = false;
    public static GameManager_Level2 Instance;
    public Image[] lifeIcons; 

    [Header("Sounds")]
    public AudioClip lifeLostSound;
    private AudioSource audioSource;

    [Header("Level 2 Rewards - Goo")]
    public int sessionGoo = 0;
    public TMPro.TextMeshProUGUI gooHudText;
    public int shieldUnlockThreshold = 15;

    [Header("Level 2 Timing")]
    public float levelDuration = 90f;
    public float bossVictoryDelay = 3f;

    [Header("Victory")]
    public AudioClip victorySound;

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
        // FIX: Ensure time is running and player can move!
        Time.timeScale = 1f;
        
        lives = startingLives;
        UpdateUI();
        UpdateGooHUD();
        
        // FIX: Enable player input when level starts!
        if (Player_Movement.Instance)
        {
            Player_Movement.Instance.enableInput(true);
            Debug.Log("[GameManager_Level2] Player input ENABLED on start!");
        }
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
        int totalGoo = PlayerPrefs.GetInt(TotalGooKey, 0) + sessionGoo;
        PlayerPrefs.SetInt(TotalGooKey, totalGoo);

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
        
        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(false);

        FreezeWorld();
        PersistGooAndCheckUnlock();
        
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
        
        isBossFight = true;

        Debug.Log("[GameManager_Level2] Time's up! Checking for boss transition...");
        
        AlienSpawner alienSpawner = FindFirstObjectByType<AlienSpawner>();
        if (alienSpawner != null)
            alienSpawner.enabled = false;

        CleanupOffScreenAliens();

        if (LevelTransition.Instance && LevelTransition.Instance.AreAllEnemiesCleared())
        {
            Debug.Log("[GameManager_Level2] All enemies cleared! Starting transition to boss...");
            PersistGooAndCheckUnlock();
            LevelTransition.Instance.StartTransition();
        }
        else
        {
            Debug.Log("[GameManager_Level2] Enemies still on screen, waiting for clear...");
            StartCoroutine(WaitForEnemiesClear());
        }
    }

    void CleanupOffScreenAliens()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        float halfHeight = cam.orthographicSize;
        float topOfScreen = cam.transform.position.y + halfHeight;

        Alien[] aliens = FindObjectsByType<Alien>(FindObjectsSortMode.None);
        int destroyedCount = 0;

        foreach (var alien in aliens)
        {
            if (alien.transform.position.y > topOfScreen + 0.5f)
            {
                Destroy(alien.gameObject);
                destroyedCount++;
            }
        }

        if (destroyedCount > 0)
        {
            Debug.Log($"[GameManager_Level2] Cleaned up {destroyedCount} off-screen aliens.");
        }
    }

    System.Collections.IEnumerator WaitForEnemiesClear()
    {
        while (true)
        {
            if (LevelTransition.Instance && LevelTransition.Instance.AreAllEnemiesCleared())
            {
                Debug.Log("[GameManager_Level2] Enemies cleared! Starting transition...");
                PersistGooAndCheckUnlock();
                LevelTransition.Instance.StartTransition();
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void FreezeWorld()
    {
        ScrollingBackground[] backgrounds = FindObjectsByType<ScrollingBackground>(FindObjectsSortMode.None);
        foreach (var bg in backgrounds)
        {
            bg.enabled = false;
        }

        AlienSpawner alienSpawner = FindFirstObjectByType<AlienSpawner>();
        if (alienSpawner != null)
            alienSpawner.enabled = false;

        Alien[] aliens = FindObjectsByType<Alien>(FindObjectsSortMode.None);
        foreach (var alien in aliens)
        {
            Destroy(alien.gameObject);
        }

        AlienBullet[] alienBullets = FindObjectsByType<AlienBullet>(FindObjectsSortMode.None);
        foreach (var bullet in alienBullets)
        {
            Destroy(bullet.gameObject);
        }

        Goo[] goos = FindObjectsByType<Goo>(FindObjectsSortMode.None);
        foreach (var goo in goos)
        {
            Destroy(goo.gameObject);
        }
    }

    public void BossDefeated()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (victorySound && audioSource)
            audioSource.PlayOneShot(victorySound);

        Debug.Log("[GameManager_Level2] Boss defeated! Victory!");

        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(false);

        PersistGooAndCheckUnlock();
        StartCoroutine(ReturnToMenuAfterVictory());
    }

    System.Collections.IEnumerator ReturnToMenuAfterVictory()
    {
        Debug.Log($"[GameManager_Level2] Returning to menu in {bossVictoryDelay} seconds...");
        
        yield return new WaitForSeconds(bossVictoryDelay);

        Debug.Log("[GameManager_Level2] Loading MainMenu...");
        SceneManager.LoadScene("MainMenu");
    }
}