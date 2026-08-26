using UnityEngine;

namespace BanRaiValley.Farming
{
    /// <summary>
    /// Defines the public contract for any crop instance placed in the world.
    /// Implemented by components that manage a single crop tile's lifecycle.
    /// </summary>
    public interface ICropInstance
    {
        /// <summary>Tilemap cell position occupied by this crop instance.</summary>
        Vector3Int CellPos { get; }

        /// <summary>The ScriptableObject data asset that defines this crop's configuration.</summary>
        CropDataSO CropData { get; }

        /// <summary>The crop's current lifecycle state.</summary>
        CropState CurrentState { get; }

        /// <summary>Zero-based index of the growth stage the crop is currently in.</summary>
        int CurrentStageIndex { get; }

        /// <summary>Number of watered days accumulated in the current growth stage.</summary>
        int DaysInCurrentStage { get; }

        /// <summary>Returns true when the crop has reached its final growth stage and awaits player interaction.</summary>
        bool IsMature { get; }

        /// <summary>Returns true when the crop has withered and can no longer grow.</summary>
        bool IsWithered { get; }

        /// <summary>
        /// Initialises this crop instance at a given tilemap cell with the specified crop data.
        /// </summary>
        /// <param name="cellPos">The tilemap cell position to assign to this crop.</param>
        /// <param name="cropData">The crop configuration ScriptableObject driving this instance's behaviour.</param>
        void Initialize(Vector3Int cellPos, CropDataSO cropData);

        /// <summary>
        /// Advances growth logic for one in-game day.
        /// </summary>
        /// <param name="wasWatered">True if the crop's tile was watered on the day being advanced.</param>
        void AdvanceGrowthDay(bool wasWatered);

        /// <summary>Transitions the crop to the <see cref="CropState.Withered"/> state.</summary>
        void SetWithered();

        /// <summary>
        /// Resets the crop to its regrowth stage as defined by <see cref="CropDataSO.RegrowStageIndex"/>.
        /// Only valid for regrowable crops.
        /// </summary>
        void ResetToRegrowth();
    }
}
