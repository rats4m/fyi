using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
public class AimbotController : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float aimSpeed = 20f;
    [SerializeField] private float aimbotFieldOfView = 20f;
    [SerializeField] private float maxTargetDistance = 100f;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private bool instantSnap = true;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    
    private PlayerLookController playerLookController;
    private PlayerInput playerInput;
    private InputAction aimbotAction;
    private Transform lockedTarget;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        playerInput = GetComponent<PlayerInput>();
        aimbotAction = playerInput.actions.FindAction("Aimbot");
        playerLookController = GetComponent<PlayerLookController>();

        if (aimbotAction == null || playerCamera == null || playerLookController == null)
        {
            Debug.LogError("AimbotController: Input Action, Player Camera, or PlayerLookController is not configured correctly.", this);
            this.enabled = false;
        }
    }
    
    void LateUpdate()
    {
        // Find a target only if the aimbot key is pressed.
        if (aimbotAction.IsPressed())
        {
            lockedTarget = FindBestTarget();
        }
        else
        {
            lockedTarget = null;
        }

        // If we have a valid target, override the look controller and perform the aimbot.
        if (lockedTarget != null)
        {
            playerLookController.IsLookOverridden = true;
            PerformAimbot(lockedTarget);
        }
        else
        {
            // Otherwise, ensure the look controller is not overridden.
            playerLookController.IsLookOverridden = false;
        }
    }

    private void OnDisable()
    {
        // Safety check to release the override if this script is disabled.
        if (playerLookController != null)
        {
            playerLookController.IsLookOverridden = false;
        }
    }

    private void PerformAimbot(Transform target)
    {
        Vector3 targetPoint = target.GetComponent<Collider>().bounds.center;
        Vector3 directionToTarget = (targetPoint - playerCamera.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        Quaternion bodyRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        Quaternion cameraRotation = Quaternion.Euler(targetRotation.eulerAngles.x, 0, 0);

        if (instantSnap)
        {
            transform.rotation = bodyRotation;
            playerCamera.transform.localRotation = cameraRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, Time.deltaTime * aimSpeed);
            playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, cameraRotation, Time.deltaTime * aimSpeed);
        }
    }

    // ... (FindBestTarget and IsTargetStillValid methods remain unchanged) ...
    private Transform FindBestTarget()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, maxTargetDistance, targetLayerMask);
        Transform bestTarget = null;
        float bestTargetScore = float.MaxValue;

        foreach (Collider targetCollider in potentialTargets)
        {
            if (targetCollider.transform.root == this.transform.root)
                continue;

            Vector3 directionToTarget = (targetCollider.bounds.center - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
            if (angle > aimbotFieldOfView)
                continue;

            if (!Physics.Raycast(playerCamera.transform.position, directionToTarget, out RaycastHit hit, maxTargetDistance) || hit.collider != targetCollider)
                continue;
            
            if (angle < bestTargetScore)
            {
                bestTarget = targetCollider.transform;
                bestTargetScore = angle;
            }
        }
        return bestTarget;
    }

    private bool IsTargetStillValid(Transform target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = (target.GetComponent<Collider>().bounds.center - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
        if (angle > aimbotFieldOfView) return false;

        if (!Physics.Raycast(playerCamera.transform.position, directionToTarget, out RaycastHit hit, maxTargetDistance) || hit.transform != target)
        {
            return false;
        }
        return true;
    }
}