using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.8f;
    public float fadeSpeed = 2f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifetime); // auto-destroy after playing
    }

    void Update()
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * fadeSpeed);
            sr.color = c;
        }
    }
}
