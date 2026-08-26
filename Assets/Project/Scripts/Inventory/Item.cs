using UnityEngine;
using BanRaiValley.Farming;

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

    [Header("Combat")]
    [SerializeField] private ItemAttackData _attackData = ItemAttackData.DefaultUnarmed;

    /// <summary>Attack parameters defined for this item.</summary>
    public ItemAttackData AttackData => _attackData;

    [Header("Farming / Seed Data")]
    [Tooltip("Crop growth configuration asset for this seed item. " +
             "Only required when ItemType is Seed. Leave null for non-seed items.")]
    [SerializeField] private CropDataSO _cropData;

    /// <summary>
    /// Crop growth configuration if this item is a seed.
    /// Returns null for non-seed items.
    /// </summary>
    public CropDataSO CropData => _cropData;
}

public enum ItemType
{
    Equipment,
    Tool,
    Weapon,
    Seed
}
