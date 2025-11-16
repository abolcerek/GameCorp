using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Transition State")]
    public bool isInTransition = false;
    public bool isWaitingForChoice = false;

    [Header("Backgrounds - Important!")]
    [Tooltip("Assign BOTH background objects, or just use a single static background")]
    public ScrollingBackground[] backgroundScrollers;  // Changed to array for both backgrounds
    public Sprite blackHoleSprite;
    public Sprite bossFightSprite;
    
    [Header("Alternative: Use Static Backgrounds")]
    [Tooltip("If checked, creates a static overlay instead of changing scrolling backgrounds")]
    public bool useStaticOverlay = true;
    public GameObject staticBackgroundOverlay;  // Assign a full-screen image

    [Header("UI Elements")]
    public GameObject transitionUI;           // Panel for the choice
    public TextMeshProUGUI transitionText;    // "Press C to Continue or R to Return"
    public Image fadeImage;                   // Black image for fade in/out

    [Header("Settings")]
    public float fadeSpeed = 1f;
    public string bossSceneName = "Level2_Boss";
    public bool useSameSceneForBoss = true;

    private bool fadingOut = false;
    private float fadeAlpha = 0f;

    public static LevelTransition Instance;

    void Awake()
    {
        Instance = this;
        
        // Make sure transition UI is hidden at start
        if (transitionUI)
            transitionUI.SetActive(false);
        
        // Make sure text is hidden at start too
        if (transitionText)
            transitionText.gameObject.SetActive(false);
        
        if (fadeImage)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // Hide static overlay if using it
        if (staticBackgroundOverlay)
            staticBackgroundOverlay.SetActive(false);
    }

    void Update()
    {
        // Handle fade effect
        if (fadingOut)
        {
            fadeAlpha += fadeSpeed * Time.deltaTime;
            if (fadeImage)
            {
                Color c = fadeImage.color;
                c.a = fadeAlpha;
                fadeImage.color = c;
            }
        }

        // Handle player choice during transition
        if (isWaitingForChoice)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ContinueToBoss();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ReturnToMenu();
            }
        }
    }

    public void StartTransition()
    {
        if (isInTransition) return;

        isInTransition = true;
        Debug.Log("[LevelTransition] Starting transition to black hole...");

        // Disable player input
        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(false);

        // Change to black hole background
        if (useStaticOverlay)
        {
            // Use static overlay approach
            ShowStaticBackground(blackHoleSprite);
        }
        else
        {
            // Change scrolling backgrounds (changes ALL background objects)
            ChangeAllBackgrounds(blackHoleSprite);
        }

        // Show choice UI after a brief delay
        Invoke(nameof(ShowChoice), 1f);
    }

    void ShowStaticBackground(Sprite sprite)
    {
        if (staticBackgroundOverlay)
        {
            staticBackgroundOverlay.SetActive(true);
            Image overlayImage = staticBackgroundOverlay.GetComponent<Image>();
            if (overlayImage)
            {
                overlayImage.sprite = sprite;
                overlayImage.color = Color.white; // Make sure it's visible
            }
            
            // Stop scrolling backgrounds
            StopAllScrolling();
            
            Debug.Log($"[LevelTransition] Showing static overlay: {sprite.name}");
        }
        else
        {
            Debug.LogWarning("[LevelTransition] No static overlay assigned! Falling back to scrolling backgrounds.");
            ChangeAllBackgrounds(sprite);
        }
    }

    void ChangeAllBackgrounds(Sprite newSprite)
    {
        if (backgroundScrollers == null || backgroundScrollers.Length == 0)
        {
            Debug.LogWarning("[LevelTransition] No background scrollers assigned!");
            return;
        }

        // Change ALL background objects to the same sprite
        foreach (var bg in backgroundScrollers)
        {
            if (bg != null && newSprite)
            {
                bg.ChangeSprite(newSprite);
            }
        }

        Debug.Log($"[LevelTransition] Changed {backgroundScrollers.Length} backgrounds to: {newSprite.name}");
    }

    void StopAllScrolling()
    {
        if (backgroundScrollers == null) return;

        foreach (var bg in backgroundScrollers)
        {
            if (bg != null)
            {
                bg.enabled = false; // Stop scrolling
            }
        }
    }

    void ShowChoice()
    {
        isWaitingForChoice = true;

        // Show UI
        if (transitionUI)
            transitionUI.SetActive(true);

        // Explicitly activate the text GameObject if it's separate
        if (transitionText)
        {
            transitionText.gameObject.SetActive(true);
            transitionText.text = "You Have Encountered a Black Hole! \n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nPress C to Continue\n\nPress R to Return to Menu";
            
            // Force it to render on top
            Canvas.ForceUpdateCanvases();
            
            Debug.Log($"[LevelTransition] Text activated: {transitionText.gameObject.name}, Active: {transitionText.gameObject.activeInHierarchy}");
        }

        Debug.Log("[LevelTransition] Waiting for player choice...");
    }

    void ContinueToBoss()
    {
        Debug.Log("[LevelTransition] Player chose to continue!");
        isWaitingForChoice = false;

        // Hide choice UI
        if (transitionUI)
            transitionUI.SetActive(false);

        if (useSameSceneForBoss)
        {
            StartBossFightInScene();
        }
        else
        {
            fadingOut = true;
            Invoke(nameof(LoadBossScene), 1f / fadeSpeed);
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("[LevelTransition] Player chose to return to menu.");
        isWaitingForChoice = false;

        // Hide choice UI
        if (transitionUI)
            transitionUI.SetActive(false);

        // Progress was already saved in GameOverByTimer

        // Fade out and load menu
        fadingOut = true;
        Invoke(nameof(LoadMainMenu), 1f / fadeSpeed);
    }

    void StartBossFightInScene()
    {
        Debug.Log("[LevelTransition] Starting boss fight in same scene!");

        // Change to boss fight background
        if (useStaticOverlay)
        {
            ShowStaticBackground(bossFightSprite);
        }
        else
        {
            ChangeAllBackgrounds(bossFightSprite);
        }

        // Re-enable player input
        if (Player_Movement.Instance)
            Player_Movement.Instance.enableInput(true);

        // Signal to spawn boss
        var bossSpawner = FindFirstObjectByType(typeof(BossSpawner)) as BossSpawner;
        if (bossSpawner != null)
        {
            bossSpawner.SpawnBoss();
        }
        else
        {
            Debug.LogWarning("[LevelTransition] No BossSpawner found in scene! Add BossSpawner when ready for boss fight.");
        }
    }

    void LoadBossScene()
    {
        SceneManager.LoadScene(bossSceneName);
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public bool AreAllEnemiesCleared()
    {
        Alien[] aliens = FindObjectsByType<Alien>(FindObjectsSortMode.None);
        AlienBullet[] alienBullets = FindObjectsByType<AlienBullet>(FindObjectsSortMode.None);

        bool cleared = aliens.Length == 0 && alienBullets.Length == 0;
        
        if (!cleared)
        {
            Debug.Log($"[LevelTransition] Enemies remaining: {aliens.Length} aliens, {alienBullets.Length} bullets");
        }
        else
        {
            Debug.Log("[LevelTransition] All enemies cleared!");
        }
        
        return cleared;
    }
}