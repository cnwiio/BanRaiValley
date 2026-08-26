using Lean.Pool;
using UnityEngine;

/// <summary>
/// Placed on a mature crop prefab or its interaction detector. When <see cref="TriggerAwakening"/>
/// is called (e.g. by the player's harvest action), this component:
/// <list type="number">
///   <item>Optionally despawns the static crop visual via <see cref="OnClearPlant"/> on the EventBus (skipped for regrowable crops).</item>
///   <item>Spawns the awakened monster prefab from the LeanPool at the same world position.</item>
///   <item>Calls <see cref="PlantBrain.Awaken"/> to start the AI uprooting sequence.</item>
///   <item>Raises <see cref="OnPlantAwakenedEvent"/> so other systems can react.</item>
/// </list>
/// </summary>
public class AwakenedCropHarvestTrigger : MonoBehaviour
{
    #region Serialized Fields

    [Header("Spawning")]
    [Tooltip("Prefab of the awakened plant monster. Must have a PlantBrain component and be registered with LeanPool. " +
             "Can be overridden at runtime via SetMonsterPrefab() from CropDataSO.")]
    [SerializeField] private GameObject _awakenedMonsterPrefab;

    #endregion


    #region Fields

    private Vector3Int _cellPos;
    private bool _isAwakened;

    #endregion


    #region Public Methods

    /// <summary>
    /// Binds this trigger to its farming grid cell position.
    /// Must be called by the owning crop or planting system immediately after spawning.
    /// </summary>
    /// <param name="cellPos">The grid cell this crop occupies, used to clear the farm tile on awakening.</param>
    public void Initialize(Vector3Int cellPos)
    {
        _cellPos    = cellPos;
        _isAwakened = false;
    }

    /// <summary>
    /// Overrides the awakened monster prefab at runtime using the crop's <see cref="CropDataSO"/> data.
    /// Only applies the override when <paramref name="monsterPrefab"/> is non-null,
    /// so the Inspector-assigned fallback is preserved for crops without a data asset.
    /// </summary>
    /// <param name="monsterPrefab">The monster prefab to spawn. Must be registered with LeanPool.</param>
    public void SetMonsterPrefab(GameObject monsterPrefab)
    {
        if (monsterPrefab != null)
            _awakenedMonsterPrefab = monsterPrefab;
    }

    /// <summary>
    /// Initiates the crop-to-monster transformation.
    /// Idempotent — subsequent calls after the first are silently ignored.
    /// </summary>
    /// <param name="isRegrowable">
    /// When true the crop tile is kept alive for regrowth, so <see cref="OnClearPlant"/> is NOT raised.
    /// When false the tile is cleared from the farm grid immediately.
    /// </param>
    public void TriggerAwakening(bool isRegrowable = false)
    {
        if (_isAwakened)
            return;

        _isAwakened = true;

        if (!isRegrowable)
        {
            // Non-regrowable: remove the static crop visual so the tile returns to tilled state.
            EventBus<OnClearPlant>.Raise(new OnClearPlant
            {
                CellPos = _cellPos,
            });
        }
        // Regrowable: the owning CropInstance handles ResetToRegrowth() — tile stays active.

        // Spawn the monster instance from the pool at this crop's world transform.
        if (_awakenedMonsterPrefab == null)
        {
            Debug.LogError("[AwakenedCropHarvestTrigger] No awakened monster prefab assigned — cannot spawn monster.");
            return;
        }

        GameObject monster = LeanPool.Spawn(
            _awakenedMonsterPrefab,
            transform.position,
            transform.rotation
        );

        if (monster == null)
        {
            Debug.LogError($"[AwakenedCropHarvestTrigger] LeanPool failed to spawn {_awakenedMonsterPrefab.name}.");
            return;
        }

        // Start the awakening animation / invulnerability sequence.
        PlantBrain brain = monster.GetComponent<PlantBrain>();
        if (brain != null)
        {
            brain.Awaken();
        }
        else
        {
            Debug.LogError($"[AwakenedCropHarvestTrigger] Spawned monster '{monster.name}' is missing a PlantBrain component.");
        }

        // Notify global listeners (e.g. quest system, audio, VFX).
        EventBus<OnPlantAwakenedEvent>.Raise(new OnPlantAwakenedEvent
        {
            PlantInstance = monster,
            CellPos       = _cellPos,
            WorldPosition = transform.position,
        });
    }

    #endregion
}
