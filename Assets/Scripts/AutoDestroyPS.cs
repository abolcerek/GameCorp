using UnityEngine;

public class AutoDestroyPS : MonoBehaviour
{
    void Start() {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration + 0.2f);
    }
}
