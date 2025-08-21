using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(Slider))]
public class PlayerHealthUI : MonoBehaviour
{
    private Slider healthSlider;
    private PlayerHealth localPlayerHealth;

    void Awake()
    {
        // Get the Slider component attached to this GameObject.
        healthSlider = GetComponent<Slider>();
    }

    void Update()
    {
        // If we haven't found the local player's health component yet, try to find it.
        if (localPlayerHealth == null)
        {
            // Check if the NetworkManager is ready and if a local player object exists.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                localPlayerHealth = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerHealth>();
                if (localPlayerHealth != null)
                {
                    // Once found, initialize the slider's max value.
                    // Note: In your PlayerHealth script, maxHealth is a private variable.
                    // For this to work, you'll need to make it public or create a public getter.
                    // Let's assume a default max health of 100 for now.
                    healthSlider.maxValue = 100;
                }
            }
        }

        // If we have a valid reference to the player's health, update the slider.
        if (localPlayerHealth != null)
        {
            healthSlider.value = localPlayerHealth.CurrentHealth.Value;
        }
    }
}