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

    [Header("Follow Boss")]
    public Transform bossTransform;
    public Vector3 offset = new Vector3(0, 1.5f, 0);  // Above boss

    private int maxHealth;
    private int currentHealth;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (!healthSlider)
            healthSlider = GetComponent<Slider>();

        if (!fillImage && healthSlider)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
    }

    void LateUpdate()
    {
        // Follow boss position
        if (bossTransform && mainCamera)
        {
            Vector3 worldPos = bossTransform.position + offset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }
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
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void SetBoss(Transform boss)
    {
        bossTransform = boss;
    }
}