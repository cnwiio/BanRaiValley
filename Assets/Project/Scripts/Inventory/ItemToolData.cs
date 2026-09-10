using UnityEngine;

public enum ToolType
{
    None = 0,
    Hoe = 1,
    WateringCan = 2,
    Axe = 3,
    Pickaxe = 4
}

public enum ToolTier
{
    Basic = 0,
    Copper = 1,
    Silver = 2,
    Gold = 3,
    Iridium = 4
}

public enum ToolAreaPattern
{
    Single = 0,
    ForwardLine = 1,
    CenteredSquare = 2
}

/// <summary>
/// Holds upgrade tier, pattern shape, and stamina parameters for a tool item.
/// Attach via <see cref="Item.ToolData"/> on any Item ScriptableObject.
/// </summary>
[System.Serializable]
public struct ItemToolData
{
    public ToolType toolType;
    public ToolTier tier;
    public ToolAreaPattern patternShape;

    [Tooltip("Dimension of the pattern (e.g. 1 for 1 tile, 3 for 3-tile line or 3x3 square, 5 for 5-tile line or 5x5 square)")]
    public int patternDimension;

    public float staminaCost;

    /// <summary>
    /// Default configuration for a basic Hoe tool.
    /// </summary>
    public static ItemToolData DefaultHoe => new ItemToolData
    {
        toolType = ToolType.Hoe,
        tier = ToolTier.Basic,
        patternShape = ToolAreaPattern.ForwardLine,
        patternDimension = 1,
        staminaCost = 1f
    };

    /// <summary>
    /// Default configuration for a basic Watering Can tool.
    /// </summary>
    public static ItemToolData DefaultWateringCan => new ItemToolData
    {
        toolType = ToolType.WateringCan,
        tier = ToolTier.Basic,
        patternShape = ToolAreaPattern.CenteredSquare,
        patternDimension = 1,
        staminaCost = 1f
    };
}
