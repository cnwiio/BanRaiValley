using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Only gameplay")]
    public ItemType type;
    //public Vector2Int range = new Vector2Int(5, 4);

    [Header("Only UI")]
    public bool stackable = true;
    public int MaxStack = 1;

    [Header("Both")]
    public Sprite image;
}

public enum ItemType
{
    Equipment,
    Tool,
    Weapon,
    Seed
}
