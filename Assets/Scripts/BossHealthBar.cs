using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;  // Optional: "100/100"

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    private int maxHealth;
    private int currentHealth;

    void Start()
    {
        if (!healthSlider)
            healthSlider = GetComponent<Slider>();

        if (!fillImage && healthSlider)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        Debug.Log("[BossHealthBar] Health bar initialized at top of screen");
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        currentHealth = max;

        if (healthSlider)
        {
            healthSlider.maxValue = max;
            healthSlider.value = max;
        }

        UpdateHealthText();
        Debug.Log($"[BossHealthBar] Max health set to {max}");
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        if (healthSlider)
            healthSlider.value = health;

        UpdateHealthColor();
        UpdateHealthText();
    }

    void UpdateHealthColor()
    {
        if (!fillImage) return;

        float percent = (float)currentHealth / maxHealth;

        if (percent > 0.5f)
        {
            // Green to yellow
            fillImage.color = Color.Lerp(halfHealthColor, fullHealthColor, (percent - 0.5f) * 2f);
        }
        else
        {
            // Yellow to red
            fillImage.color = Color.Lerp(lowHealthColor, halfHealthColor, percent * 2f);
        }
    }

    void UpdateHealthText()
    {
        if (healthText)
        {
            healthText.text = $"Boss: {currentHealth}/{maxHealth}";
        }
    }
}