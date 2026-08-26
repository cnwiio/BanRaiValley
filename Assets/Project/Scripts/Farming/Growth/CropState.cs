namespace BanRaiValley.Farming
{
    /// <summary>
    /// Represents the current lifecycle state of a crop instance.
    /// </summary>
    public enum CropState
    {
        /// <summary>Crop is actively progressing through growth stages.</summary>
        Growing,

        /// <summary>Crop has reached its final stage and is ready for player interaction (awaken or harvest).</summary>
        Mature,

        /// <summary>Crop has withered due to an incompatible season or neglect; can be cleared by the player.</summary>
        Withered,

        /// <summary>Crop has been awakened into a monster or harvested by the player.</summary>
        Harvested
    }
}
