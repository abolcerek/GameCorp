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
    [Header("Shop UI - Missile")]
    public Image shopMissileIcon;            // Unlocked missile image (shown when unlocked)
    public Image shopMissileLockedIcon;      // Locked/with-slash image (shown when locked)
    public Image shopShardIcon;              // Small shard sprite shown next to progress
    public TextMeshProUGUI shopShardProgressText; // e.g., "14/25"
    public Button shopEquipMissileButton;    // optional: enable only when unlocked

    [Header("Shop UI - Shield")]
    public Image shopShieldIcon;             // Unlocked shield image (shown when unlocked)
    public Image shopShieldLockedIcon;       // Locked shield image (shown when locked)
    public Image shopGooIcon;                // Small goo sprite shown next to progress
    public TextMeshProUGUI shopGooProgressText; // e.g., "8/15"
    public Button shopEquipShieldButton;     // optional: enable only when unlocked

    [Header("Rewards Rules")]
    public int missileUnlockThreshold = 25;
    public int shieldUnlockThreshold = 15;   // NEW: Goo needed for shield

    // PlayerPrefs keys (must match GameManager)
    const string TotalShardsKey = "TotalShards";
    const string TotalGooKey = "TotalGoo";           // NEW
    const string MissilesUnlockedKey = "MissilesUnlocked";
    const string ShieldUnlockedKey = "ShieldUnlocked";  // NEW

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
        // ===== MISSILE (Shards) =====
        int totalShards = PlayerPrefs.GetInt(TotalShardsKey, 0);
        bool missileUnlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1 || totalShards >= missileUnlockThreshold;

        // Cap the progress display at the requirement (e.g., 25/25)
        int shardsShown = Mathf.Min(totalShards, missileUnlockThreshold);

        // Toggle which missile image is visible
        if (shopMissileIcon)       shopMissileIcon.gameObject.SetActive(missileUnlocked);
        if (shopMissileLockedIcon) shopMissileLockedIcon.gameObject.SetActive(!missileUnlocked);

        // Progress "current/required" next to a shard icon
        if (shopShardIcon) shopShardIcon.gameObject.SetActive(true);

        if (shopShardProgressText)
        {
            shopShardProgressText.text = $"{shardsShown}/{missileUnlockThreshold}";
        }

        // Optional: Equip button only when unlocked
        if (shopEquipMissileButton)
            shopEquipMissileButton.interactable = missileUnlocked;

        // ===== SHIELD (Goo) =====
        int totalGoo = PlayerPrefs.GetInt(TotalGooKey, 0);
        bool shieldUnlocked = PlayerPrefs.GetInt(ShieldUnlockedKey, 0) == 1 || totalGoo >= shieldUnlockThreshold;

        // Cap the progress display at the requirement (e.g., 15/15)
        int gooShown = Mathf.Min(totalGoo, shieldUnlockThreshold);

        // Toggle which shield image is visible
        if (shopShieldIcon)       shopShieldIcon.gameObject.SetActive(shieldUnlocked);
        if (shopShieldLockedIcon) shopShieldLockedIcon.gameObject.SetActive(!shieldUnlocked);

        // Progress "current/required" next to a goo icon
        if (shopGooIcon) shopGooIcon.gameObject.SetActive(true);

        if (shopGooProgressText)
        {
            shopGooProgressText.text = $"{gooShown}/{shieldUnlockThreshold}";
        }

        // Optional: Equip button only when unlocked
        if (shopEquipShieldButton)
            shopEquipShieldButton.interactable = shieldUnlocked;
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

    }

    void SetActiveSafe(GameObject go, bool state)
    {
        if (!go) return;
        if (go.activeSelf != state) go.SetActive(state);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TotalShards");
        PlayerPrefs.DeleteKey("TotalGoo");
        PlayerPrefs.DeleteKey("MissilesUnlocked");
        PlayerPrefs.DeleteKey("ShieldUnlocked");
        PlayerPrefs.Save();

        RefreshShopUI();

        Debug.Log("Progress reset!");
    }
}