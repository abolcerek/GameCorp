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

    void Start()
    {
        ShowMain();
        if (level2Button) level2Button.interactable = false;
        if (level3Button) level3Button.interactable = false;
    }

    // === Main navigation ===
    public void ShowMain()
    {
        panelMain.SetActive(true);
        panelLevelSelect.SetActive(false);
        if (panelControls) panelControls.SetActive(false);
        if (panelShop) panelShop.SetActive(false);
        if (panelCustomize) panelCustomize.SetActive(false);
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

    // === Optional other panels ===
    public void ShowControls()  { ToggleOne(panelControls); }
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
