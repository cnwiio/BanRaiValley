using System.Collections.Generic;
using BanRaiValley.Time;
using UnityEngine;

namespace BanRaiValley.Farming
{
    /// <summary>
    /// ScriptableObject asset that defines all static configuration for a single crop type:
    /// growth stages, seasonal compatibility, withering, regrowth, harvesting, and awakened monster spawning.
    /// Create via <c>BanRaiValley/Farming/Crop Data</c> in the Unity Asset Menu.
    /// </summary>
    [CreateAssetMenu(fileName = "CropData_", menuName = "BanRaiValley/Farming/Crop Data")]
    public class CropDataSO : ScriptableObject
    {
        #region Serialized Fields

        [Header("Identity")]
        [Tooltip("Unique identifier used by save systems and lookups. Must not change after initial setup.")]
        public string CropId;

        [Tooltip("Display name shown to the player in UI panels.")]
        public string CropName;

        [TextArea(2, 4)]
        [Tooltip("Short description of this crop shown in the Crop Encyclopedia or item tooltip.")]
        public string Description;


        [Header("Seasonal Compatibility")]
        [Tooltip("Seasons during which this crop grows normally. Outside these seasons the crop will wither.")]
        public List<Season> CompatibleSeasons = new List<Season>();


        [Header("Growth Stages")]
        [Tooltip("Ordered list of growth stage configurations. Index 0 is the seedling stage. " +
                 "The last entry is the mature stage.")]
        public List<CropStageData> Stages = new List<CropStageData>();

        [Tooltip("Prefab displayed when the crop has entered the Withered state.")]
        public GameObject WitheredPrefab;


        [Header("Awakening")]
        [Tooltip("Monster prefab spawned when the player chooses to awaken this crop instead of harvesting it. " +
                 "Leave null if this crop cannot be awakened.")]
        public GameObject AwakenedMonsterPrefab;


        [Header("Regrowth")]
        [Tooltip("When true, this crop regrows after being harvested rather than being removed from the tile.")]
        public bool IsRegrowable;

        [Tooltip("The stage index the crop resets to after a successful harvest. Only used when IsRegrowable is true.")]
        public int RegrowStageIndex;

        [Tooltip("Number of watered days required to complete the regrowth stage. Only used when IsRegrowable is true.")]
        public int RegrowDays;


        [Header("Yield")]
        [Tooltip("Item granted to the player's inventory upon a normal harvest.")]
        public Item HarvestItem;

        [Tooltip("Seed item dropped when the crop is cleared while withered, or obtained through processing.")]
        public Item SeedItem;

        #endregion


        #region Properties

        /// <summary>Zero-based index of the final (mature) stage in <see cref="Stages"/>.</summary>
        public int FinalStageIndex => Stages.Count > 0 ? Stages.Count - 1 : 0;

        /// <summary>
        /// Returns true when the specified season is listed in <see cref="CompatibleSeasons"/>.
        /// </summary>
        /// <param name="season">The season to test for compatibility.</param>
        /// <returns>True if the crop can grow during <paramref name="season"/>; false otherwise.</returns>
        public bool IsSeasonCompatible(Season season) =>
            CompatibleSeasons != null && CompatibleSeasons.Contains(season);

        #endregion
    }
}
