using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelLevelSelect;
    public GameObject panelControls;
    public GameObject panelShop;
    public GameObject panelCustomize;

    [Header("Buttons (optional)")]
    public Button level2Button;
    public Button level3Button;

    [Header("Gameplay")]
    public string gameplaySceneName = "Game";

    // ===== SHOP UI =====
    [Header("Shop UI")]
    public Image shopMissileIcon;            // Unlocked missile image (shown when unlocked)
    public Image shopMissileLockedIcon;      // Locked/with-slash image (shown when locked)
    public Image shopShardIcon;              // Small shard sprite shown next to progress
    public TextMeshProUGUI shopShardProgressText; // e.g., "14/25"
    public Button shopEquipMissileButton;    // optional: enable only when unlocked

    [Header("Rewards Rules")]
    public int missileUnlockThreshold = 25;

    // PlayerPrefs keys (must match GameManager)
    const string TotalShardsKey = "TotalShards";
    const string MissilesUnlockedKey = "MissilesUnlocked";

    void OnEnable()
    {
        RefreshShopUI();
    }

    void Start()
    {
        ShowMain();
        if (level2Button) level2Button.interactable = false;
        if (level3Button) level3Button.interactable = false;
        RefreshShopUI();
    }

    void RefreshShopUI()
    {
        int total = PlayerPrefs.GetInt(TotalShardsKey, 0);
        bool unlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1 || total >= missileUnlockThreshold;

        // Cap the progress display at the requirement (e.g., 25/25)
        int shown = Mathf.Min(total, missileUnlockThreshold);

        // Toggle which missile image is visible
        if (shopMissileIcon)       shopMissileIcon.gameObject.SetActive(unlocked);
        if (shopMissileLockedIcon) shopMissileLockedIcon.gameObject.SetActive(!unlocked);

        // Progress "current/required" next to a shard icon
        if (shopShardIcon) shopShardIcon.gameObject.SetActive(true); // always show icon in this row

        if (shopShardProgressText)
        {
            shopShardProgressText.text = $"{shown}/{missileUnlockThreshold}";
        }

        // Optional: Equip button only when unlocked
        if (shopEquipMissileButton)
            shopEquipMissileButton.interactable = unlocked;
    }


    // ---------- Navigation ----------
    public void ShowMain()
    {
        SetActiveSafe(panelMain, true);
        SetActiveSafe(panelLevelSelect, false);
        SetActiveSafe(panelControls, false);
        SetActiveSafe(panelShop, false);
        SetActiveSafe(panelCustomize, false);
    }

    public void ShowShop()
    {
        SetActiveSafe(panelMain, false);
        SetActiveSafe(panelLevelSelect, false);
        SetActiveSafe(panelControls, false);
        SetActiveSafe(panelShop, true);
        SetActiveSafe(panelCustomize, false);

        RefreshShopUI();
    }

    public void Play()
    {
        SetActiveSafe(panelMain, false);
        SetActiveSafe(panelLevelSelect, true);
        SetActiveSafe(panelControls, false);
        SetActiveSafe(panelShop, false);
        SetActiveSafe(panelCustomize, false);
    }

    public void ShowControls()
    {
        SetActiveSafe(panelMain, false);
        SetActiveSafe(panelLevelSelect, false);
        SetActiveSafe(panelControls, true);
        SetActiveSafe(panelShop, false);
        SetActiveSafe(panelCustomize, false);
    }

    public void BackToMain() => ShowMain();

    public void SelectLevel1() { SceneManager.LoadScene(gameplaySceneName); }
    public void SelectLevel2_Placeholder() { Debug.Log("Level 2 coming soon!"); }
    public void SelectLevel3_Placeholder() { Debug.Log("Level 3 coming soon!"); }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void SetActiveSafe(GameObject go, bool state)
    {
        if (!go) return;
        if (go.activeSelf != state) go.SetActive(state);
    }

        public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TotalShards");
        PlayerPrefs.DeleteKey("MissilesUnlocked");
        PlayerPrefs.Save();

        RefreshShopUI();

        Debug.Log("Progress reset!");
    }
}
