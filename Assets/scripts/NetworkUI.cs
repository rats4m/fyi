using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to add this

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject buttonPanel;

    void Awake()
    {

        hostButton.onClick.AddListener(() =>
        {
            // Set the connection data before starting
            NetworkManager.Singleton.StartHost();
            buttonPanel.SetActive(false);
        });

        clientButton.onClick.AddListener(() =>
        {
            // Set the connection data before starting
            NetworkManager.Singleton.StartClient();
            buttonPanel.SetActive(false);
        });
    }
}