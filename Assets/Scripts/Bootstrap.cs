using UnityEngine;
using UnityEngine.EventSystems;

public class LevelBootstrap : MonoBehaviour
{
    void Start()
    {
        // Make sure gameplay actually runs
        Time.timeScale = 1f;

        // Prevent UI from "owning" the keyboard when level loads
        if (EventSystem.current) 
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Don't lock cursor for 2D game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[Bootstrap] Level initialized - Input should be active");
        Debug.Log($"[Bootstrap] Time.timeScale: {Time.timeScale}");
        Debug.Log($"[Bootstrap] Player_Movement canMove: {Player_Movement.Instance?.canMove}");
    }
    
    void Update()
    {
        // Temporary debug - remove after fixing
        if (Input.anyKeyDown)
        {
            Debug.Log($"[Bootstrap] Key pressed! Time: {Time.time}");
        }
    }
}