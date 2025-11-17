using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Transition State")]
    public bool isInTransition = false;
    public bool isWaitingForChoice = false;

    [Header("Simple Background System")]
    public GameObject blackHoleBackground;  // Assign your black hole background GameObject
    public GameObject bossFightBackground;  // Assign your boss fight background GameObject

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

        // Make sure both backgrounds are hidden at start
        if (blackHoleBackground)
            blackHoleBackground.SetActive(false);
        
        if (bossFightBackground)
            bossFightBackground.SetActive(false);
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

        // Hide scrolling backgrounds
        HideScrollingBackgrounds();

        // Show black hole background
        if (blackHoleBackground)
        {
            blackHoleBackground.SetActive(true);
            Debug.Log("[LevelTransition] Black hole background shown");
        }

        // Show choice UI after a brief delay
        Invoke(nameof(ShowChoice), 1f);
    }

    void HideScrollingBackgrounds()
    {
        // Find and disable ALL scrolling backgrounds
        ScrollingBackground[] backgrounds = FindObjectsByType<ScrollingBackground>(FindObjectsSortMode.None);
        foreach (var bg in backgrounds)
        {
            if (bg != null)
            {
                bg.gameObject.SetActive(false);
            }
        }
        Debug.Log($"[LevelTransition] Hid {backgrounds.Length} scrolling backgrounds");
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
            transitionText.text = "You Have Encountered a Black Hole!\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nPress C to Continue\n\nPress R to Return to Menu";
            
            // Force it to render on top
            Canvas.ForceUpdateCanvases();
            
            Debug.Log($"[LevelTransition] Text activated");
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

        // Reset camera to ensure proper view
        Camera mainCam = Camera.main;
        if (mainCam)
        {
            mainCam.orthographicSize = 5f;
            mainCam.transform.position = new Vector3(0, 0, -10);
            Debug.Log("[LevelTransition] Camera reset to default view");
        }

        // Hide black hole background
        if (blackHoleBackground)
        {
            blackHoleBackground.SetActive(false);
            Debug.Log("[LevelTransition] Black hole background hidden");
        }

        // Show boss fight background
        if (bossFightBackground)
        {
            bossFightBackground.SetActive(true);
            Debug.Log("[LevelTransition] Boss fight background shown");
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
            Debug.LogWarning("[LevelTransition] No BossSpawner found in scene!");
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