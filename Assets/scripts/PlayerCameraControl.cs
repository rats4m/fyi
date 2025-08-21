using Unity.Netcode;
using UnityEngine;

// This script ensures that only the camera and audio listener for the local player are active.
public class PlayerCameraControl : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        // On any client, when a player object spawns, this code runs.
        // The IsOwner flag is true only for the player object controlled by this client.
        if (IsOwner)
        {
            // If this is the local player, ensure their camera and listener are enabled.
            playerCamera.enabled = true;
            audioListener.enabled = true;
        }
        else
        {
            // If this is a remote player (controlled by someone else),
            // disable their camera and listener on this client.
            playerCamera.enabled = false;
            audioListener.enabled = false;
        }
    }
}