using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Only gameplay")]
    public ItemType type;
    public Vector3 spawnOffset;
    //public Vector2Int range = new Vector2Int(5, 4);

    [Header("Only UI")]
    public bool stackable = true;
    public int MaxStack = 1;

    [Header("Both")]
    public Sprite image;
    public GameObject prefab;

    [Header("Shop")] 
    public int Price;

    [Header("Combat")] 
    public ItemAttackData attackData;
}

public enum ItemType
{
    Equipment,
    Tool,
    Weapon,
    Seed
}
