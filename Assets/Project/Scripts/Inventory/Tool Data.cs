using UnityEngine;

[System.Serializable]
public struct ToolData
{
    public ToolAreaPattern toolPattern;
    
    [Tooltip("Dimension of the pattern (e.g. 1 for 1 tile, 3 for 3-tile line or 3x3 square, 5 for 5-tile line or 5x5 square)")]
    public int patternDimension;
}

public enum ToolAreaPattern
{
    Single = 0,
    ForwardLine = 1,
    CenteredSquare = 2
}
