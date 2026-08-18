using Lean.Pool;
using UnityEngine;

/// <summary>
/// Scene-level manager that listens for global Plant AI events and coordinates
/// the return of dead plant monsters to the LeanPool. Also re-broadcasts tile-clear
/// signals so the farming grid knows a cell is free once the monster is defeated.
/// No direct references to <see cref="PlantBrain"/> instances are held;
/// all coupling occurs exclusively through the EventBus.
/// </summary>
public class PlantSpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Despawn Settings")]
    [Tooltip("Additional delay in seconds after receiving OnPlantDiedEvent before returning the instance to the pool. "
           + "Should be >= PlantBrain's _despawnDelaySec so the death animation can finish.")]
    [SerializeField] private float _poolReturnDelaySec = 4.5f;

    #endregion

    #region Unity Messages

    private void OnEnable()
    {
        EventBus<OnPlantDiedEvent>.Subscribe(OnPlantDied);
    }

    private void OnDisable()
    {
        EventBus<OnPlantDiedEvent>.Unsubscribe(OnPlantDied);
    }

    #endregion

    #region Event Handlers

    private void OnPlantDied(OnPlantDiedEvent evt)
    {
        if (evt.PlantInstance == null)
        {
            return;
        }

        // Re-raise OnClearPlant for this cell so PlantManager removes any residual
        // entry and the farming grid tile becomes available for re-planting.
        EventBus<OnClearPlant>.Raise(new OnClearPlant
        {
            CellPos = evt.CellPos,
        });

        // Return the monster GameObject to the pool after the death animation delay.
        StartCoroutine(ReturnToPoolRoutine(evt.PlantInstance, _poolReturnDelaySec));
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Waits for the specified duration then returns the GameObject to the LeanPool.
    /// </summary>
    /// <param name="instance">The monster GameObject to despawn.</param>
    /// <param name="delaySec">Seconds to wait before despawning, allowing death animations to complete.</param>
    private System.Collections.IEnumerator ReturnToPoolRoutine(GameObject instance, float delaySec)
    {
        yield return new UnityEngine.WaitForSeconds(delaySec);

        // Guard: instance may have already been returned to pool or destroyed externally.
        if (instance != null && instance.activeSelf)
        {
            LeanPool.Despawn(instance);
        }
    }

    #endregion
}
