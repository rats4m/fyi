using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
public class PlayerLookController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float lookSensitivity = 100f;
    
    // --- NEW: Public flag to allow other scripts to override this one ---
    public bool IsLookOverridden { get; set; } = false;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private float xRotation = 0f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // Ensure cursor is locked when this script is active
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        // When the player is destroyed or disabled, unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // --- NEW: If look is overridden by another system (like the aimbot), do nothing. ---
        if (IsLookOverridden)
        {
            return;
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}