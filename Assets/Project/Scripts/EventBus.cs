using System;
using UnityEngine;

public interface IEvent {}

public static class EventBus<T> where T : struct, IEvent
{
    public static event Action<T> OnEvent;

    public static void Subscribe(Action<T> handler)
    {
        OnEvent += handler;
    }

    public static void Unsubscribe(Action<T> handler)
    {
        OnEvent -= handler;
    }

    public static void Raise(T evt)
    {
        OnEvent?.Invoke(evt);
    }
}

// --------------------------------------------------------
// Global Game Events
// --------------------------------------------------------

//public struct AddScoreEvent : IEvent
//{
//    public int Score;
//}
//public struct GameStartedEvent : IEvent { }

public struct OnJumpEvent : IEvent { }

#region Inventory UI
public struct OnUIBeginDragEvent : IEvent 
{
    public int Index;
    public IInventory Inventory;
    public InventorySlotUI SlotUI;
}
public struct OnUIDragEvent : IEvent 
{
    public UnityEngine.Vector2 Position;
}
public struct OnUIEndDragEvent : IEvent 
{
    public IInventory Inventory;
    public InventorySlotUI SlotUI;
}
public struct OnUIDropEvent : IEvent 
{
    public int Index;
    public IInventory Inventory;
    public InventorySlotUI SlotUI;

}

public struct InventoryUIRefreshEvent : IEvent { }

public struct InventoryToggleEvent : IEvent { }

public struct OnHotbarSelectEvent : IEvent
{
    public int Index;
}
public struct OnHotbarScrollActionEvent : IEvent
{
    public int value;
}

public struct OnHotbarChangeEvent : IEvent
{
    public SlotData slotData;
}
#endregion

public struct ChangeActionMap : IEvent
{
    public ActionMapType MapType;
}

public struct OnPrimaryActionEvent : IEvent { }
public struct OnSecondaryActionEvent : IEvent { }

#region Hoe Event
public struct OnTillingImpactEvent : IEvent
{
    public GameObject prefabs;
    public Vector3 Position;
    public float YRotation;
    public Vector3Int CellPos;
}
public struct OnTileClearEvent : IEvent 
{
    public Vector3Int CellPos;
}
public struct OnStartTillingEvent : IEvent 
{
}
#endregion

public struct StartPreviewEvent : IEvent
{
    public GameObject prefabs;
    public PreviewState previewState;
}
public struct PreviewingEvent : IEvent
{
    public Vector3 Position;
    public bool IsValid;
    public float YRotation;
}

public struct EndPreviewEvent : IEvent
{

}

public struct OnRotateActionEvent : IEvent { }
public struct OnRotateFarmEvent : IEvent 
{
    public float YRotation;
}

public struct OnDeleteActionEvent : IEvent { }

public struct OnStartWateringEvent : IEvent { }
public struct OnWateringEvent : IEvent
{
    public Vector3Int CellPos;
    public Material Material;
}

public struct OnStartPlantingEvent : IEvent { }

public struct OnPlantingEvent : IEvent
{
    public GameObject Prefab;
    public Vector3 Position;
    public Vector3Int CellPos;
}

public struct OnClearPlant : IEvent
{
    public Vector3Int CellPos;
}

#region Plant AI Events

/// <summary>
/// Raised when a plant entity awakens and becomes an active AI combatant.
/// </summary>
public struct OnPlantAwakenedEvent : IEvent
{
    /// <summary>The plant's root GameObject.</summary>
    public GameObject PlantInstance;

    /// <summary>Grid cell the plant occupies.</summary>
    public Vector3Int CellPos;

    /// <summary>World-space position of the plant.</summary>
    public Vector3 WorldPosition;
}

/// <summary>
/// Raised every time a plant AI transitions between behavioural states.
/// </summary>
public struct OnPlantStateChangedEvent : IEvent
{
    /// <summary>The plant's root GameObject.</summary>
    public GameObject PlantInstance;

    /// <summary>The state the plant was in before the transition.</summary>
    public PlantAIState PreviousState;

    /// <summary>The state the plant has entered.</summary>
    public PlantAIState NewState;
}

/// <summary>
/// Raised when a plant entity receives a damage hit.
/// </summary>
public struct OnPlantDamagedEvent : IEvent
{
    /// <summary>The plant's root GameObject.</summary>
    public GameObject PlantInstance;

    /// <summary>Full context of the damage applied.</summary>
    public DamageData DamageData;

    /// <summary>HP value immediately after the damage is applied.</summary>
    public float CurrentHp;

    /// <summary>Maximum HP of the plant (used to calculate health percentage).</summary>
    public float MaxHp;
}

/// <summary>
/// Raised when a plant entity's HP reaches zero and the death sequence begins.
/// </summary>
public struct OnPlantDiedEvent : IEvent
{
    /// <summary>The plant's root GameObject.</summary>
    public GameObject PlantInstance;

    /// <summary>World-space position at the moment of death.</summary>
    public Vector3 Position;

    /// <summary>Grid cell the plant occupied.</summary>
    public Vector3Int CellPos;
}

/// <summary>
/// Raised each time a plant completes an attack action against its target.
/// </summary>
public struct OnPlantAttackExecutedEvent : IEvent
{
    /// <summary>The plant's root GameObject.</summary>
    public GameObject PlantInstance;

    /// <summary>Transform of the entity that was attacked.</summary>
    public Transform TargetTransform;

    /// <summary>Raw damage amount dealt by this attack.</summary>
    public float DamageAmount;
}

#endregion
