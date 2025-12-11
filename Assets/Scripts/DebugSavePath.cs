using UnityEngine;
using System.IO;

public class DebugSavePath : MonoBehaviour
{
    // EDIT THIS: Change "savefile.json" to the actual name of your rewards file
    public string fileName = "savefile.json"; 

    void Start()
    {
        // Construct the path exactly as Unity sees it on this OS
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        
        Debug.Log("--- PATH DEBUGGER START ---");
        Debug.Log($"[OS]: {SystemInfo.operatingSystem}");
        Debug.Log($"[Persistent Path]: {Application.persistentDataPath}");
        Debug.Log($"[Target File Path]: {fullPath}");

        // 1. Check if the file exists
        if (File.Exists(fullPath))
        {
            Debug.Log($"[STATUS]: SUCCESS - File found.");

            // 2. Try to read the file to ensure permissions are okay
            try 
            {
                string content = File.ReadAllText(fullPath);
                // Print a snippet to verify it's not empty
                string preview = content.Length > 50 ? content.Substring(0, 50) + "..." : content;
                Debug.Log($"[CONTENT PREVIEW]: {preview}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[READ ERROR]: File exists but cannot be read. Permission issue? Error: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[STATUS]: FAILURE - File NOT found at: {fullPath}");
            Debug.LogWarning("If this is a fresh install, the file may not have been created yet.");
        }
        
        Debug.Log("--- PATH DEBUGGER END ---");
    }
}