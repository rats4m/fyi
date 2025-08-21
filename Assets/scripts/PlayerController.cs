using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -12f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private CharacterController controller;
    
    private Vector3 playerVelocity;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (IsOwner)
        {
            moveAction = playerInput.actions.FindAction("Move");
            jumpAction = playerInput.actions.FindAction("Jump");

            if (moveAction == null || jumpAction == null)
            {
                Debug.LogError("PlayerController: Could not find a 'Move' or 'Jump' action.");
                this.enabled = false;
            }
        }
        else
        {
            playerInput.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isJumpPressed = jumpAction.triggered;

        SubmitInputsServerRpc(moveInput, isJumpPressed);
    }
    
    [ServerRpc]
    private void SubmitInputsServerRpc(Vector2 moveInput, bool isJumpPressed)
    {
        // --- THIS ENTIRE BLOCK IS REFACTORED FOR ROBUSTNESS ---

        // First, check if the player is grounded on the server.
        bool isGrounded = controller.isGrounded;

        // If grounded, reset any lingering downward velocity to keep them stuck to the ground.
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // Calculate horizontal movement based on player's orientation.
        Vector3 horizontalMove = (transform.forward * moveInput.y + transform.right * moveInput.x) * moveSpeed;

        // If a jump was requested and the server agrees the player is grounded, apply jump velocity.
        if (isJumpPressed && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity to our vertical velocity.
        playerVelocity.y += gravity * Time.deltaTime;

        // Combine horizontal and vertical motion into a single vector.
        Vector3 finalVelocity = horizontalMove + new Vector3(0, playerVelocity.y, 0);

        // Apply the final, combined movement in a single call. This is more stable.
        controller.Move(finalVelocity * Time.deltaTime);
    }
}