using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;
    private CharacterController controller;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        default,
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer)
        {
            return;
        }

        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - damageAmount, 0);

        if (CurrentHealth.Value <= 0)
        {
            // Instead of just logging, we now call the Respawn method.
            Respawn();
        }
    }

    private void Respawn()
    {
        // This method is only ever called on the server.
        Vector3 spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();

        // It is best practice to disable the CharacterController when teleporting the transform directly.
        controller.enabled = false;
        transform.position = spawnPoint;
        controller.enabled = true;

        // Reset health.
        CurrentHealth.Value = maxHealth;
    }
}