using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject tracerPrefab; // <-- Reference to the tracer

    [Header("Settings")]
    [SerializeField] private float fireRange = 100f;

    private PlayerInput playerInput;
    private InputAction fireAction;

    public override void OnNetworkSpawn()
    {
        // This script is only active for the local player who owns this object.
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions.FindAction("Fire");

        if (fireAction == null)
        {
            Debug.LogError("PlayerShooting: Could not find a 'Fire' action. Check your Input Action Asset.", this);
            this.enabled = false;
        }
    }

    void Update()
    {
        // 'triggered' is true for the single frame the Fire button is pressed.
        if (fireAction != null && fireAction.triggered)
        {
            // Tell the server we want to fire.
            FireServerRpc();
        }
    }

    [ServerRpc]
    private void FireServerRpc()
    {
        // Offset the raycast start point slightly in front of the camera to avoid hitting the player.
        Vector3 rayStart = playerCamera.transform.position + playerCamera.transform.forward * 2f; // Adjust the offset as needed.

        // Server performs the authoritative raycast.
        Ray ray = new Ray(rayStart, playerCamera.transform.forward);
        RaycastHit hitInfo;
        Vector3 endPoint = ray.GetPoint(fireRange); // Assume we miss, calculate the full range.

        if (Physics.Raycast(ray, out hitInfo, fireRange))
        {
            endPoint = hitInfo.point; // If we hit something, the end point is the impact point.
            if (hitInfo.collider.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(25);
            }
        }

        // Tell all clients to spawn the visual tracer from the start to the end point.
        ShowShotVisualClientRpc(ray.origin, endPoint);
    }

    [ClientRpc]
    private void ShowShotVisualClientRpc(Vector3 startPoint, Vector3 endPoint)
    {
        // This runs on all clients to create the visual effect.
        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab, startPoint, Quaternion.identity);
            LineRenderer lr = tracer.GetComponent<LineRenderer>();

            lr.startWidth = 0.05f; // Adjust the width as needed.
            lr.endWidth = 0.05f;

            lr.SetPosition(0, startPoint);
            lr.SetPosition(1, endPoint);
        }
    }
}