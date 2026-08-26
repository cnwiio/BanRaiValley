using System.Collections.Generic;
using BanRaiValley.Farming;
using BanRaiValley.Time;
using UnityEngine;

/// <summary>
/// Central manager for all active crops across the farming grid.
/// Subscribes to day/season events to drive daily growth evaluation,
/// season-based withering, and morning soil hydration reset.
/// </summary>
/// <remarks>
/// Event-driven — no <c>Update</c> polling.
/// All crop advancement and withering is triggered exclusively via
/// <see cref="OnNewDayStarted"/> and <see cref="OnSeasonChanged"/>.
/// </remarks>
public class CropGrowthManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Grid")]
    [Tooltip("Shared ScriptableObject reference that points to the active FarmingGrid at runtime.")]
    [SerializeField] private FarmingGridReference _farmingGridReference;

    [Header("Crop Prefabs")]
    [Tooltip("Base prefab for all crop instances. Must have a CropInstance component attached.")]
    [SerializeField] private GameObject _cropInstanceBasePrefab;

    #endregion


    #region Fields

    private readonly Dictionary<Vector3Int, CropInstance> _activeCrops =
        new Dictionary<Vector3Int, CropInstance>();

    #endregion


    #region Unity Messages

    private void OnEnable()
    {
        EventBus<OnPlantingEvent>.Subscribe(OnPlanting);
        EventBus<OnClearPlant>.Subscribe(OnClearPlant);
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDayStarted);
        EventBus<OnSeasonChangedEvent>.Subscribe(OnSeasonChanged);
    }

    private void OnDisable()
    {
        EventBus<OnPlantingEvent>.Unsubscribe(OnPlanting);
        EventBus<OnClearPlant>.Unsubscribe(OnClearPlant);
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDayStarted);
        EventBus<OnSeasonChangedEvent>.Unsubscribe(OnSeasonChanged);
    }

    #endregion


    #region Event Handlers

    /// <summary>
    /// Spawns and initialises a new <see cref="CropInstance"/> when the player plants a seed.
    /// Registers it in the active crop registry and raises <see cref="OnCropPlantedEvent"/>.
    /// Skips the spawn if <see cref="OnPlantingEvent.CropData"/> is null (legacy / non-growth planting).
    /// </summary>
    private void OnPlanting(OnPlantingEvent evt)
    {
        if (evt.CropData == null)
        {
            // Legacy planting path without crop data — spawn the raw prefab if provided.
            if (evt.Prefab != null)
                Instantiate(evt.Prefab, evt.Position, Quaternion.identity);

            return;
        }

        GameObject prefabToSpawn = _cropInstanceBasePrefab != null ? _cropInstanceBasePrefab : evt.Prefab;
        if (prefabToSpawn == null)
        {
            Debug.LogError("[CropGrowthManager] No crop instance prefab assigned and OnPlantingEvent carries no fallback prefab.");
            return;
        }

        GameObject cropGO = Instantiate(prefabToSpawn, evt.Position, Quaternion.identity);
        CropInstance cropInstance = cropGO.GetComponent<CropInstance>();

        if (cropInstance == null)
        {
            Debug.LogError($"[CropGrowthManager] Spawned prefab '{prefabToSpawn.name}' is missing a CropInstance component.");
            Destroy(cropGO);
            return;
        }

        cropInstance.Initialize(evt.CellPos, evt.CropData);
        _activeCrops[evt.CellPos] = cropInstance;

        EventBus<OnCropPlantedEvent>.Raise(new OnCropPlantedEvent
        {
            CropInstance = cropGO,
            CellPos      = evt.CellPos,
            CropData     = evt.CropData,
        });
    }

    /// <summary>
    /// Removes and destroys the crop instance registered at <see cref="OnClearPlant.CellPos"/>.
    /// Safe to call even if no crop is registered at that cell.
    /// </summary>
    private void OnClearPlant(OnClearPlant evt)
    {
        if (!_activeCrops.TryGetValue(evt.CellPos, out CropInstance crop))
            return;

        _activeCrops.Remove(evt.CellPos);

        if (crop != null)
            Destroy(crop.gameObject);
    }

    /// <summary>
    /// Morning tick handler — evaluates each active crop against season and watering rules,
    /// advances eligible crops, then resets soil hydration for the new day.
    /// </summary>
    /// <remarks>
    /// Order is critical: hydration is <em>read</em> before it is <em>reset</em>.
    /// </remarks>
    private void OnNewDayStarted(OnNewDayStartedEvent evt)
    {
        Season currentSeason = evt.NewDateTime.Season;
        IFarmingGrid grid = _farmingGridReference != null ? _farmingGridReference.Grid : null;

        // Collect keys into a snapshot list to allow safe modification during iteration
        // (SetWithered raises events that could theoretically modify _activeCrops via listeners).
        var cropKeys = new List<Vector3Int>(_activeCrops.Keys);

        foreach (Vector3Int cellPos in cropKeys)
        {
            if (!_activeCrops.TryGetValue(cellPos, out CropInstance crop) || crop == null)
                continue;

            // Season incompatibility withers any non-withered crop immediately.
            if (!crop.CropData.IsSeasonCompatible(currentSeason))
            {
                if (!crop.IsWithered)
                    crop.SetWithered();

                continue;
            }

            // Season is compatible — advance growth if the crop can still grow.
            if (!crop.IsMature && !crop.IsWithered)
            {
                bool wasWatered = grid != null && grid.IsWatered(cellPos);
                crop.AdvanceGrowthDay(wasWatered);
            }
        }

        // Reset hydration AFTER growth evaluation so watered state was correctly read above.
        if (grid != null)
        {
            int resetCount = grid.ResetDailyHydration();
            EventBus<OnSoilHydrationResetEvent>.Raise(new OnSoilHydrationResetEvent
            {
                ResetTilesCount = resetCount,
            });
        }
    }

    /// <summary>
    /// Withers all active crops that are incompatible with the newly started season.
    /// Handles mid-day season transitions that occur without a day rollover.
    /// </summary>
    private void OnSeasonChanged(OnSeasonChangedEvent evt)
    {
        foreach (var cropEntry in _activeCrops)
        {
            CropInstance crop = cropEntry.Value;

            if (crop == null || crop.IsWithered)
                continue;

            if (!crop.CropData.IsSeasonCompatible(evt.NewSeason))
                crop.SetWithered();
        }
    }

    #endregion
}
