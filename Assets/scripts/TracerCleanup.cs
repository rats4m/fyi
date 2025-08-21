using UnityEngine;

public class TracerCleanup : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.1f;

    void Start()
    {
        // Destroy this GameObject after 'lifetime' seconds.
        Destroy(gameObject, lifetime);
    }
}