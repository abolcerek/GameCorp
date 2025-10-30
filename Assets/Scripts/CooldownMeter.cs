using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownMeter : MonoBehaviour
{
    public Player_Shooting shooter;
    public Slider slider;
    public TextMeshProUGUI label;   // optional "COOLING…" text
    public Image fillImage;         // slider's Fill image (optional color change)

    [Header("Colors")]
    public Color cold = new Color(0.2f, 0.9f, 0.3f);
    public Color warm = new Color(1f, 0.82f, 0.25f);
    public Color hot  = new Color(1f, 0.25f, 0.25f);

    // Optional visual exaggeration (UI only)
    [Range(0.3f, 1.2f)] public float displayExponent = 0.85f;

    void Reset()
    {
        slider = GetComponent<Slider>();
        if (!fillImage) fillImage = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (!shooter || !slider) return;

        float p = shooter.HeatPercent;
        float pUI = Mathf.Pow(p, displayExponent);   // UI-only curve
        slider.value = pUI;

        if (fillImage)
        {
            // Blend cold -> warm -> hot by (true) heat percent so color tracks real state
            Color mid = Color.Lerp(cold, warm, Mathf.InverseLerp(0f, 0.6f, p));
            fillImage.color = Color.Lerp(mid, hot, Mathf.InverseLerp(0.6f, 1f, p));
        }

        if (label)
        {
            if (shooter.IsOverheated)
                label.text = $"COOLING… ({shooter.SecondsUntilReady:0.0}s)";
            else
                label.text = ""; // or $"{Mathf.RoundToInt(p*100)}%"
        }
    }
}
