using Lean.Pool;
using UnityEngine;

/// <summary>
/// Placed on a mature crop prefab or its interaction detector. When <see cref="TriggerAwakening"/>
/// is called (e.g. by the player's harvest action), this component:
/// <list type="number">
///   <item>Despawns the static crop visual via <see cref="OnClearPlant"/> on the EventBus.</item>
///   <item>Spawns the awakened monster prefab from the LeanPool at the same world position.</item>
///   <item>Calls <see cref="PlantBrain.Awaken"/> to start the AI uprooting sequence.</item>
///   <item>Raises <see cref="OnPlantAwakenedEvent"/> so other systems can react.</item>
/// </list>
/// </summary>
public class AwakenedCropHarvestTrigger : MonoBehaviour
{
    #region Serialized Fields

    [Header("Spawning")]
    [Tooltip("Prefab of the awakened plant monster. Must have a PlantBrain component and be registered with LeanPool.")]
    [SerializeField] private GameObject _awakenedMonsterPrefab;

    #endregion

    #region Fields

    private Vector3Int _cellPos;
    private bool       _isAwakened;

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
    /// Initiates the crop-to-monster transformation.
    /// Idempotent — subsequent calls after the first are silently ignored.
    /// </summary>
    public void TriggerAwakening()
    {
        if (_isAwakened)
        {
            return;
        }

        _isAwakened = true;

        // 1. Remove the static crop visual from PlantManager so the tile returns to tilled state.
        EventBus<OnClearPlant>.Raise(new OnClearPlant
        {
            CellPos = _cellPos,
        });

        // 2. Spawn the monster instance from the pool at this crop's world transform.
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

        // 3. Start the awakening animation / invulnerability sequence.
        PlantBrain brain = monster.GetComponent<PlantBrain>();
        if (brain != null)
        {
            brain.Awaken();
        }
        else
        {
            Debug.LogError($"[AwakenedCropHarvestTrigger] Spawned monster '{monster.name}' is missing a PlantBrain component.");
        }

        // 4. Notify global listeners (e.g. quest system, audio, VFX).
        EventBus<OnPlantAwakenedEvent>.Raise(new OnPlantAwakenedEvent
        {
            PlantInstance = monster,
            CellPos       = _cellPos,
            WorldPosition = transform.position,
        });
    }

    #endregion
}
