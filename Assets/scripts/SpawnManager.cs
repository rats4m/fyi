using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Create a static instance so it can be easily accessed from other scripts
    public static SpawnManager Instance { get; private set; }

    [Header("Map Boundaries")]
    [Tooltip("The maximum X-axis value for a spawn point.")]
    [SerializeField] private float mapBoundsX = 24f;
    [Tooltip("The maximum Z-axis value for a spawn point.")]
    [SerializeField] private float mapBoundsZ = 24f;

    [Header("Exclusion Zone")]
    [Tooltip("The center of the area to exclude from spawning.")]
    [SerializeField] private Vector3 exclusionZoneCenter = Vector3.zero;
    [Tooltip("The size (radius on X and Z) of the exclusion zone.")]
    [SerializeField] private Vector3 exclusionZoneSize = new Vector2(5f, 5f);

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Vector3 spawnPoint;
        int attempts = 0;

        do
        {
            float randomX = Random.Range(-mapBoundsX, mapBoundsX);
            float randomZ = Random.Range(-mapBoundsZ, mapBoundsZ);
            spawnPoint = new Vector3(randomX, 1f, randomZ); // Y=1 to spawn just above the ground
            attempts++;
            if (attempts > 100)
            {
                Debug.LogError("Failed to find a valid spawn point after 100 attempts!");
                return new Vector3(0, 1, 0); // Fallback spawn point
            }
        } 
        while (IsInExclusionZone(spawnPoint));

        return spawnPoint;
    }

    private bool IsInExclusionZone(Vector3 point)
    {
        // Check if the point is within the rectangular exclusion zone on the X-Z plane
        return point.x >= exclusionZoneCenter.x - exclusionZoneSize.x &&
               point.x <= exclusionZoneCenter.x + exclusionZoneSize.x &&
               point.z >= exclusionZoneCenter.z - exclusionZoneSize.z &&
               point.z <= exclusionZoneCenter.z + exclusionZoneSize.z;
    }
}