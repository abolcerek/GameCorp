using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Universal video player for scene transitions.
/// Use this for Intro scenes, transition scenes, cutscenes, etc.
/// Just drag your VideoPlayer and set the next scene name!
/// </summary>
public class VideoSceneManager : MonoBehaviour
{
    [Header("Required")]
    [Tooltip("The VideoPlayer component in your scene")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("Scene to load after video finishes")]
    public string nextSceneName = "MainMenu";

    [Header("Playback Settings")]
    [Tooltip("Minimum time to display scene (fallback if video fails)")]
    public float minimumDisplayTime = 3f;
    
    [Tooltip("Maximum time to wait for video to load before giving up")]
    public float maxPrepareTime = 10f;
    
    [Tooltip("Allow skipping video with any key press")]
    public bool allowSkipping = true;
    
    [Tooltip("Delay before allowing skip (prevents accidental skips)")]
    public float skipDelay = 3f;

    [Header("Debug")]
    public bool verboseLogging = true;

    private bool hasTransitioned = false;
    private bool canSkip = false;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        Log("Video scene started");

        if (videoPlayer == null)
        {
            LogError("No VideoPlayer assigned! Assign it in the Inspector.");
            StartCoroutine(FallbackTimer());
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            LogError("Next scene name is empty! Set it in the Inspector.");
            return;
        }

        // Configure video player for best build compatibility
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane; // Ensure it renders
        videoPlayer.isLooping = false; // CRITICAL: Don't loop the video!

        // Subscribe to video events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;

        // Start video preparation
        StartCoroutine(PrepareAndPlayVideo());
    }

    IEnumerator PrepareAndPlayVideo()
    {
        Log("Preparing video...");

        // Prepare the video
        videoPlayer.Prepare();

        float timer = 0f;
        
        // Wait for video to prepare with timeout
        while (!videoPlayer.isPrepared && timer < maxPrepareTime)
        {
            timer += Time.unscaledDeltaTime; // Use unscaled time in case timeScale is 0
            yield return null;
        }

        // Check if preparation succeeded
        if (!videoPlayer.isPrepared)
        {
            LogWarning($"Video failed to prepare after {maxPrepareTime}s. Using fallback timer.");
            StartCoroutine(FallbackTimer());
            yield break;
        }

        Log($"Video prepared successfully after {timer:F2}s");
        
        // Play the video
        videoPlayer.Play();

        // Enable skipping after delay
        if (allowSkipping)
        {
            yield return new WaitForSeconds(skipDelay);
            canSkip = true;
            Log("Skipping now enabled");
        }
    }

    void OnVideoStarted(VideoPlayer source)
    {
        Log($"Video started playing (Length: {source.length:F2}s)");
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Log("Video preparation completed");
    }

    void OnVideoFinished(VideoPlayer source)
    {
        // Only transition if we've passed the minimum display time
        if (Time.time - startTime < minimumDisplayTime)
        {
            Log($"Video ended too early (at {Time.time - startTime:F2}s, minimum is {minimumDisplayTime}s). Waiting...");
            StartCoroutine(DelayedTransition());
            return;
        }
        
        Log("Video finished playing");
        LoadNextScene();
    }
    
    IEnumerator DelayedTransition()
    {
        float elapsed = Time.time - startTime;
        float remaining = minimumDisplayTime - elapsed;
        
        if (remaining > 0)
        {
            yield return new WaitForSeconds(remaining);
        }
        
        LoadNextScene();
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        LogError($"Video error: {message}");
        StartCoroutine(FallbackTimer());
    }

    IEnumerator FallbackTimer()
    {
        Log($"Using fallback timer: {minimumDisplayTime}s");
        
        // Wait for skip delay before allowing skip
        if (allowSkipping)
        {
            yield return new WaitForSeconds(skipDelay);
            canSkip = true;
            Log("Skipping now enabled (fallback mode)");
        }
        
        // Wait remaining time
        float remainingTime = Mathf.Max(0, minimumDisplayTime - skipDelay);
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (hasTransitioned) return;
        hasTransitioned = true;

        Log($"Loading scene: {nextSceneName}");
        
        // Clean up
        CleanupVideoPlayer();
        
        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    void Update()
    {
        // Handle skipping
        if (canSkip && Input.anyKeyDown)
        {
            Log("Video skipped by user input");
            
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            LoadNextScene();
        }
    }

    void CleanupVideoPlayer()
    {
        if (videoPlayer == null) return;

        // Unsubscribe from events
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.started -= OnVideoStarted;
    }

    void OnDestroy()
    {
        CleanupVideoPlayer();
    }

    // Logging helpers
    void Log(string message)
    {
        if (verboseLogging)
            Debug.Log($"[VideoSceneManager] {message}");
    }

    void LogWarning(string message)
    {
        Debug.LogWarning($"[VideoSceneManager] {message}");
    }

    void LogError(string message)
    {
        Debug.LogError($"[VideoSceneManager] {message}");
    }
}