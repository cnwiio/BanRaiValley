using UnityEngine;

namespace BanRaiValley.Farming
{
    /// <summary>
    /// Holds configuration data for a single visual growth stage of a crop.
    /// </summary>
    [System.Serializable]
    public struct CropStageData
    {
        /// <summary>Zero-based index identifying this stage within its <see cref="CropDataSO"/> stage list.</summary>
        public int StageIndex;

        /// <summary>Number of watered days required to complete this stage and advance to the next.</summary>
        public int DaysRequired;

        /// <summary>Prefab representing the visual model displayed while the crop is in this stage.</summary>
        public GameObject StageVisualPrefab;
    }
}
