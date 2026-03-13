using UnityEngine;

/// <summary>
/// ScriptableObject defining the parameters for a single RAM Overload puzzle wave.
/// Create via: Assets > Create > RAMPuzzle > WaveConfig
/// </summary>
[CreateAssetMenu(fileName = "WaveConfig", menuName = "RAMPuzzle/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Spawn Timing")]
    [Tooltip("Seconds between corruption spawn attempts")]
    public float spawnInterval = 2.5f;

    [Header("Corruption Type Chances (0-1)")]
    [Range(0f, 1f)]
    [Tooltip("Chance any given spawn is an OVERFLOW cell (timed, urgent)")]
    public float overflowChance = 0f;

    [Range(0f, 1f)]
    [Tooltip("Chance any given spawn is a CHAIN group")]
    public float chainChance = 0f;

    [Header("Chain Settings")]
    [Tooltip("How many cells appear in a chain sequence (min 2)")]
    [Min(2)]
    public int chainLength = 2;

    [Header("Pressure")]
    [Tooltip("Base pressure increase per second (stacks with active corruptions)")]
    public float pressureIncreaseRate = 0.5f;

    [Tooltip("Extra pressure added per active corrupted/overflow cell per second")]
    public float pressurePerActiveCell = 0.15f;

    [Header("Overflow Settings")]
    [Tooltip("Seconds before an OVERFLOW cell expires and causes damage")]
    public float overflowDuration = 4f;

    [Header("Wave Completion")]
    [Tooltip("Number of cells the player must clear to finish this wave")]
    public int cellsToComplete = 20;

    [Header("Penalties")]
    [Tooltip("Pressure added when player clicks a chain cell out of order")]
    public float wrongOrderPenalty = 6f;

    [Tooltip("Pressure spike when an overflow cell expires")]
    public float overflowExpiryPressureSpike = 25f;
}
