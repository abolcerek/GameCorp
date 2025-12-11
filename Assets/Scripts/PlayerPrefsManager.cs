using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility to manage PlayerPrefs and ensure build reflects editor state.
/// Add this script to a GameObject in your MainMenu scene for easy access.
/// </summary>
public class PlayerPrefsManager : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (showDebugInfo)
        {
            DisplayCurrentProgress();
        }
    }
    
    public void DisplayCurrentProgress()
    {
        Debug.Log("========== CURRENT PLAYER PROGRESS ==========");
        Debug.Log($"Total Shards: {PlayerPrefs.GetInt("TotalShards", 0)}");
        Debug.Log($"Total Goo: {PlayerPrefs.GetInt("TotalGoo", 0)}");
        Debug.Log($"Missiles Unlocked: {PlayerPrefs.GetInt("MissilesUnlocked", 0) == 1}");
        Debug.Log($"Shield Unlocked: {PlayerPrefs.GetInt("ShieldUnlocked", 0) == 1}");
        Debug.Log($"Level 2 Unlocked: {PlayerPrefs.GetInt("Level2Unlocked", 0) == 1}");
        Debug.Log("============================================");
    }
    
    /// <summary>
    /// Reset all progress - useful for testing fresh builds
    /// </summary>
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey("TotalShards");
        PlayerPrefs.DeleteKey("TotalGoo");
        PlayerPrefs.DeleteKey("MissilesUnlocked");
        PlayerPrefs.DeleteKey("ShieldUnlocked");
        PlayerPrefs.DeleteKey("Level2Unlocked");
        PlayerPrefs.Save();
        
        Debug.Log("[PlayerPrefsManager] All progress RESET!");
        DisplayCurrentProgress();
    }
    
    /// <summary>
    /// Unlock everything for testing
    /// </summary>
    public void UnlockEverything()
    {
        PlayerPrefs.SetInt("TotalShards", 100);
        PlayerPrefs.SetInt("TotalGoo", 50);
        PlayerPrefs.SetInt("MissilesUnlocked", 1);
        PlayerPrefs.SetInt("ShieldUnlocked", 1);
        PlayerPrefs.SetInt("Level2Unlocked", 1);
        PlayerPrefs.Save();
        
        Debug.Log("[PlayerPrefsManager] Everything UNLOCKED!");
        DisplayCurrentProgress();
    }
    
    /// <summary>
    /// Set specific values for testing
    /// </summary>
    public void SetProgress(int shards, int goo, bool missiles, bool shield, bool level2)
    {
        PlayerPrefs.SetInt("TotalShards", shards);
        PlayerPrefs.SetInt("TotalGoo", goo);
        PlayerPrefs.SetInt("MissilesUnlocked", missiles ? 1 : 0);
        PlayerPrefs.SetInt("ShieldUnlocked", shield ? 1 : 0);
        PlayerPrefs.SetInt("Level2Unlocked", level2 ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("[PlayerPrefsManager] Progress updated!");
        DisplayCurrentProgress();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: Delete PlayerPrefs before building
    /// This ensures your build starts fresh
    /// </summary>
    [MenuItem("Tools/PlayerPrefs/Reset Before Build")]
    public static void ResetBeforeBuild()
    {
        if (EditorUtility.DisplayDialog(
            "Reset PlayerPrefs Before Build?",
            "This will delete ALL PlayerPrefs so your build starts with a fresh state.\n\nAre you sure?",
            "Yes, Reset",
            "Cancel"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[PlayerPrefsManager] PlayerPrefs RESET for build!");
            
            EditorUtility.DisplayDialog(
                "PlayerPrefs Reset Complete",
                "All PlayerPrefs have been cleared.\n\nYour next build will start with no progress.",
                "OK");
        }
    }
    
    [MenuItem("Tools/PlayerPrefs/Show Current Progress")]
    public static void ShowProgressEditor()
    {
        string message = $"Total Shards: {PlayerPrefs.GetInt("TotalShards", 0)}\n" +
                        $"Total Goo: {PlayerPrefs.GetInt("TotalGoo", 0)}\n" +
                        $"Missiles Unlocked: {PlayerPrefs.GetInt("MissilesUnlocked", 0) == 1}\n" +
                        $"Shield Unlocked: {PlayerPrefs.GetInt("ShieldUnlocked", 0) == 1}\n" +
                        $"Level 2 Unlocked: {PlayerPrefs.GetInt("Level2Unlocked", 0) == 1}";
        
        EditorUtility.DisplayDialog("Current PlayerPrefs", message, "OK");
        Debug.Log("========== CURRENT PLAYERPREFS ==========");
        Debug.Log(message);
        Debug.Log("=========================================");
    }
    
    [MenuItem("Tools/PlayerPrefs/Unlock Everything (Testing)")]
    public static void UnlockEverythingEditor()
    {
        PlayerPrefs.SetInt("TotalShards", 100);
        PlayerPrefs.SetInt("TotalGoo", 50);
        PlayerPrefs.SetInt("MissilesUnlocked", 1);
        PlayerPrefs.SetInt("ShieldUnlocked", 1);
        PlayerPrefs.SetInt("Level2Unlocked", 1);
        PlayerPrefs.Save();
        
        EditorUtility.DisplayDialog(
            "Everything Unlocked!",
            "All upgrades and levels are now unlocked for testing.",
            "OK");
        
        Debug.Log("[PlayerPrefsManager] Everything unlocked in editor!");
    }
#endif
}