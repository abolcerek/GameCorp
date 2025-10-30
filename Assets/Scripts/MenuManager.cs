using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    // Set to your gameplay scene name
    public string gameplaySceneName = "Game";

    [Header("Rewards UI (Menu)")]
    public TMPro.TextMeshProUGUI totalShardsText;
    public GameObject missilesUnlockedBadge;
    public GameObject missilesLockedBadge;

    [Header("Rewards Rules")]
    public int missileUnlockThreshold = 25;

    const string TotalShardsKey = "TotalShards";
    const string MissilesUnlockedKey = "MissilesUnlocked";


    void Start()
    {
        ShowMain();
        if (level2Button) level2Button.interactable = false;
        if (level3Button) level3Button.interactable = false;
        RefreshRewardsUI();
    }

    void RefreshRewardsUI()
    {
        int total = PlayerPrefs.GetInt(TotalShardsKey, 0);
        bool unlocked = PlayerPrefs.GetInt(MissilesUnlockedKey, 0) == 1;

        if (totalShardsText) totalShardsText.text = $"Total Shards: {total}";
        if (missilesUnlockedBadge) missilesUnlockedBadge.SetActive(unlocked);
        if (missilesLockedBadge)   missilesLockedBadge.SetActive(!unlocked);
    }


    // === Main navigation ===
    public void ShowMain()
    {
        panelMain.SetActive(true);
        panelLevelSelect.SetActive(false);
        panelControls.SetActive(false);
        if (panelShop) panelShop.SetActive(false);
        if (panelCustomize) panelCustomize.SetActive(false);
    }

    public void ShowControls()
    {
        panelMain.SetActive(false);
        panelControls.SetActive(true);
    }



    // Play opens Level Select
    public void Play()
    {
        panelMain.SetActive(false);
        panelLevelSelect.SetActive(true);
    }

    public void BackToMain() => ShowMain();

    // === Level selection ===
    public void SelectLevel1()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void SelectLevel2_Placeholder()
    {
        Debug.Log("Level 2 coming soon!");
    }

    public void SelectLevel3_Placeholder()
    {
        Debug.Log("Level 3 coming soon!");
    }

    public void ShowShop()      { ToggleOne(panelShop); }
    public void ShowCustomize() { ToggleOne(panelCustomize); }

    void ToggleOne(GameObject target)
    {
        panelMain.SetActive(false);
        panelLevelSelect.SetActive(false);
        if (panelControls) panelControls.SetActive(target == panelControls);
        if (panelShop) panelShop.SetActive(target == panelShop);
        if (panelCustomize) panelCustomize.SetActive(target == panelCustomize);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
