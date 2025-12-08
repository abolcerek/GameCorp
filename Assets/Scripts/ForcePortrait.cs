using UnityEngine;
using UnityEngine.SceneManagement;

public class ForcePortrait : MonoBehaviour
{
    [Header("Portrait Settings")]
    [Tooltip("Desired portrait aspect ratio (9:16).")]
    public int targetWidth  = 1080;
    public int targetHeight = 1920;

    [Header("Display Mode")]
    [Tooltip("Run in fullscreen mode with pillarboxing (black bars on sides).")]
    public bool fullscreen = true;
    
    [Tooltip("If fullscreen is false, run in windowed mode.")]
    public bool windowed = true;

    [Tooltip("Keep this enforcer across scene loads.")]
    public bool persistAcrossScenes = true;

    // Target aspect ratio
    private float targetAspect;
    private bool temporarilyDisabled = false;

    void Awake()
    {
        if (persistAcrossScenes)
        {
            // Only one enforcer should exist.
            var existing = FindObjectsOfType<ForcePortrait>();
            if (existing.Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
        }

        // Calculate target aspect ratio (9:16 = 0.5625)
        targetAspect = (float)targetWidth / (float)targetHeight;

        QualitySettings.vSyncCount = 1;      // optional
        Application.targetFrameRate = 120;   // optional
    }

    void Start()
    {
        ApplyFullscreenWithAspectRatio();
    }

    void Update()
    {
        // Check if screen resolution changed
        if (Screen.width != Screen.currentResolution.width || 
            Screen.height != Screen.currentResolution.height)
        {
            ApplyFullscreenWithAspectRatio();
        }
    }

    void LateUpdate()
    {
        // Ensure camera rect is maintained every frame
        // (some systems reset it)
        if (!temporarilyDisabled && fullscreen)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                float windowAspect = (float)Screen.width / (float)Screen.height;
                float currentCameraAspect = camera.rect.width * windowAspect / camera.rect.height;
                
                // If camera rect is wrong, reapply
                if (Mathf.Abs(currentCameraAspect - targetAspect) > 0.01f)
                {
                    SetupCamera();
                }
            }
        }
    }

    private void ApplyFullscreenWithAspectRatio()
    {
        if (temporarilyDisabled)
        {
            // During video scenes, reset camera to full viewport
            ResetCamera();
            return;
        }

        if (fullscreen)
        {
            // Get the native screen resolution
            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;

#if UNITY_EDITOR
            // In editor, just use current Game view resolution
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            Debug.Log($"[ForcePortrait] Editor mode - Game view: {screenWidth}x{screenHeight}");
#else
            // In builds, set fullscreen at native resolution
            Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow);
            Debug.Log($"[ForcePortrait] Build mode - Setting fullscreen: {screenWidth}x{screenHeight}");
#endif

            // Always apply camera viewport to maintain portrait aspect ratio
            SetupCamera();
        }
        else
        {
            // Windowed mode with exact dimensions
            Screen.SetResolution(targetWidth, targetHeight, windowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
            
            // Reset camera to full viewport
            ResetCamera();
        }
    }

    private void SetupCamera()
    {
        // Get the main camera
        Camera camera = Camera.main;
        if (camera == null) return;

        // Calculate current screen aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        
        Debug.Log($"[ForcePortrait] Window aspect: {windowAspect:F4}, Target aspect: {targetAspect:F4}");

        Rect rect = new Rect(0, 0, 1, 1);

        // Compare aspects to determine if we need pillarbox or letterbox
        if (windowAspect > targetAspect)
        {
            // Screen is WIDER than target (landscape screen showing portrait game)
            // Need PILLARBOX (black bars on left and right)
            rect.width = targetAspect / windowAspect;
            rect.x = (1f - rect.width) / 2f;
            Debug.Log($"[ForcePortrait] Pillarboxing: viewport width = {rect.width:F4}, x offset = {rect.x:F4}");
        }
        else
        {
            // Screen is TALLER than target (portrait screen, or very tall display)
            // Need LETTERBOX (black bars on top and bottom)
            rect.height = windowAspect / targetAspect;
            rect.y = (1f - rect.height) / 2f;
            Debug.Log($"[ForcePortrait] Letterboxing: viewport height = {rect.height:F4}, y offset = {rect.y:F4}");
        }

        camera.rect = rect;
    }

    private void ResetCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        // Full viewport
        camera.rect = new Rect(0, 0, 1, 1);
    }

    void OnPreCull()
    {
        // Clear the background before rendering (creates the black bars)
        GL.Clear(true, true, Color.black);
    }

    // Public methods for video scenes to control pillarboxing
    public void DisablePillarboxing()
    {
        temporarilyDisabled = true;
        ResetCamera();
        Debug.Log("[ForcePortrait] Pillarboxing disabled for video playback");
    }

    public void EnablePillarboxing()
    {
        temporarilyDisabled = false;
        ApplyFullscreenWithAspectRatio();
        Debug.Log("[ForcePortrait] Pillarboxing re-enabled for gameplay");
    }
}