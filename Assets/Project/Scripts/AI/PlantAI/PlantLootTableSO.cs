using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines one entry in a loot table, specifying which item may drop,
/// how many can drop, and the probability of it dropping.
/// </summary>
[System.Serializable]
public struct LootDropEntry
{
    [Tooltip("Reference to the dropped item data.")]
    public Item Item;

    [Tooltip("Minimum drop quantity (inclusive).")]
    public int MinQuantity;

    [Tooltip("Maximum drop quantity (inclusive).")]
    public int MaxQuantity;

    [Range(0f, 100f)]
    [Tooltip("Drop chance percentage (0-100%).")]
    public float DropChancePercent;
}

/// <summary>
/// Pure data container ScriptableObject that holds the loot drop table for a Plant AI monster.
/// Contains no runtime state, no scene references, and no event subscriptions.
/// </summary>
[CreateAssetMenu(fileName = "PlantLootTable", menuName = "BanRaiValley/AI/Plant Loot Table")]
public class PlantLootTableSO : ScriptableObject
{
    #region Serialized Fields

    [Header("Loot Drops")]
    [Tooltip("List of potential item drops evaluated on monster death.")]
    [SerializeField] private List<LootDropEntry> _dropEntries = new List<LootDropEntry>();

    #endregion


    #region Properties

    /// <summary>Read-only view of all configured loot drop entries.</summary>
    public IReadOnlyList<LootDropEntry> DropEntries => _dropEntries;

    #endregion


    #region Public Methods

    /// <summary>
    /// Evaluates each loot entry against a random roll and returns the list of items that dropped.
    /// This is a pure query — it produces no side effects and mutates no state.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Item"/> instances that passed their drop chance roll.
    /// Each item appears once per successful roll; quantity is determined separately by the caller
    /// using <see cref="LootDropEntry.MinQuantity"/> and <see cref="LootDropEntry.MaxQuantity"/>.
    /// Returns an empty list if no entries pass their roll.
    /// </returns>
    public List<Item> EvaluateDrops()
    {
        var droppedItems = new List<Item>();

        foreach (LootDropEntry entry in _dropEntries)
        {
            if (entry.Item == null)
            {
                continue;
            }

            float roll = Random.Range(0f, 100f);
            if (roll <= entry.DropChancePercent)
            {
                droppedItems.Add(entry.Item);
            }
        }

        return droppedItems;
    }

    #endregion
}
