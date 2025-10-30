using UnityEngine;
using UnityEngine.SceneManagement;

public class ForcePortrait : MonoBehaviour
{
    [Header("Portrait Settings")]
    [Tooltip("Desired portrait resolution (Width x Height).")]
    public int targetWidth  = 1080;
    public int targetHeight = 1920;

    [Tooltip("Run windowed. (Fullscreen can ignore portrait on desktop.)")]
    public bool windowed = true;

    [Tooltip("Prevent user from freely resizing the window into landscape.")]
    public bool enforceEveryFrame = true;

    [Tooltip("Keep this enforcer across scene loads.")]
    public bool persistAcrossScenes = true;

    // Cache last known size to avoid spamming SetResolution.
    private int lastW, lastH;

    void Awake()
    {
        if (persistAcrossScenes)
        {
            // Only one enforcer should exist.
            var existing = FindObjectsOfType<ForcePortrait>();
            if (existing.Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
        }

        QualitySettings.vSyncCount = 1;      // optional
        Application.targetFrameRate = 120;   // optional
    }

    void Start()
    {
        ApplyPortraitResolution(true);
        // In case some UI/layout needs one frame before sizing:
        Invoke(nameof(ApplyPortraitResolution), 0.05f);
    }

    void Update()
    {
        if (!enforceEveryFrame) return;

        // If window flipped to landscape (width > height), force it back.
        if (Screen.width > Screen.height || Screen.width != lastW || Screen.height != lastH)
        {
            ApplyPortraitResolution();
        }
    }

    private void ApplyPortraitResolution(bool log = false)
    {
        // Keep exact 9:16. If current height is smaller (user dragged), maintain ratio.
        int w = targetWidth;
        int h = targetHeight;

        // If user shrank the window, clamp to at least 9:16 shape.
        if (Screen.height < targetHeight)
        {
            h = Mathf.Max(Screen.height, 600); // avoid tiny windows
            w = Mathf.RoundToInt(h * 9f / 16f);
        }

        // Ensure portrait shape
        if (w > h)
        {
            int tmp = w; w = h; h = tmp; // swap if somehow reversed
        }

        if (log)
        {
            Debug.Log($"[ForcePortrait] Setting resolution to {w}x{h} windowed={windowed}");
        }

        Screen.SetResolution(w, h, !windowed ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        lastW = w; lastH = h;
    }
}
