using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [Header("Shield System")]
    public int maxShields = 2;
    private int currentShields = 0;
    private bool shieldUnlocked = false;

    [Header("Visual Feedback")]
    public float hitFlashDuration = 0.1f;
    public Color shieldHitColor = Color.cyan;
    public Color damageHitColor = Color.red;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int lives;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer)
            originalColor = spriteRenderer.color;

        // Check if shield is unlocked
        shieldUnlocked = PlayerPrefs.GetInt("ShieldUnlocked", 0) == 1;

        // Initialize shields if unlocked
        if (shieldUnlocked)
        {
            currentShields = maxShields;
            Debug.Log($"[Player_Health] Shield unlocked! Starting with {currentShields} shields.");
        }
        else
        {
            currentShields = 0;
            Debug.Log("[Player_Health] Shield not unlocked. Starting without shields.");
        }

        // Update sprite to show shields
        if (Player_Movement.Instance != null)
            Player_Movement.Instance.UpdateSprite(currentShields);
    }

    public void TakeDamage(int amount = 1)
    {
        // Check shields first
        if (currentShields > 0)
        {
            // Shield absorbs the hit
            currentShields--;
            Debug.Log($"[Player_Health] Shield absorbed hit! Shields remaining: {currentShields}");
            
            // Update sprite to show shield loss
            if (Player_Movement.Instance != null)
                Player_Movement.Instance.UpdateSprite(currentShields);
            
            // Visual feedback for shield hit
            StartCoroutine(FlashShieldHit());
            
            return; // Don't lose lives while shields are active
        }

        // No shields left - damage lives
        Debug.Log($"[Player_Health] No shields! Taking damage to lives.");
        
        // Visual feedback for life damage
        StartCoroutine(FlashDamageHit());

        // Try Level 1 GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(amount);
            lives = GameManager.Instance.CurrentLives();
            return;
        }

        // Try Level 2 GameManager
        if (GameManager_Level2.Instance != null)
        {
            GameManager_Level2.Instance.LoseLife(amount);
            lives = GameManager_Level2.Instance.CurrentLives();
            return;
        }

        Debug.LogError("[Player_Health] No GameManager found! Cannot take damage.");
    }

    System.Collections.IEnumerator FlashShieldHit()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = shieldHitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    System.Collections.IEnumerator FlashDamageHit()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = damageHitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    // Public method to get current shield count (for UI or other systems)
    public int GetShieldCount()
    {
        return currentShields;
    }

    // Public method to check if shields are active
    public bool HasShields()
    {
        return currentShields > 0;
    }
}