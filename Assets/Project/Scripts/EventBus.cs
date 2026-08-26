using System;
using UnityEngine;
using BanRaiValley.Farming;
using BanRaiValley.Time;

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
    /// <summary>Crop configuration asset driving this planting action.</summary>
    public CropDataSO CropData;
    /// <summary>Visual prefab to spawn at the planting position (legacy / preview support).</summary>
    public GameObject Prefab;
    /// <summary>World-space position of the planted tile.</summary>
    public Vector3 Position;
    /// <summary>Tilemap cell coordinate of the planted tile.</summary>
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

#region Player Combat Events

/// <summary>
/// Raised when the player inputs an attack request (button pressed).
/// The combat controller listens to this to start the attack sequence.
/// </summary>
public struct OnPlayerRequestAttackEvent : IEvent { }

/// <summary>
/// Raised after the player's attack animation / hitbox window has fired.
/// Carry the equipped item and resolved attack data for downstream systems.
/// </summary>
public struct OnPlayerAttackExecutedEvent : IEvent
{
    /// <summary>The item that was held when the attack was performed.</summary>
    public Item EquippedItem;

    /// <summary>Resolved attack parameters used for this swing.</summary>
    public ItemAttackData AttackData;

    /// <summary>World-space origin of the attack overlap-box.</summary>
    public Vector3 AttackOrigin;
}

/// <summary>
/// Raised for each valid target that was hit during a player attack sweep.
/// </summary>
public struct OnPlayerHitTargetEvent : IEvent
{
    /// <summary>The GameObject that received the hit.</summary>
    public GameObject TargetInstance;

    /// <summary>Full damage context passed to the target's IDamageable.</summary>
    public DamageData DamageData;

    /// <summary>Whether the hit reduced the target's HP to zero.</summary>
    public bool TargetDied;
}

#endregion

#region Plant Growth Events

/// <summary>
/// Raised when a new crop instance is spawned and registered on a farm tile.
/// </summary>
public struct OnCropPlantedEvent : IEvent
{
    /// <summary>The root GameObject representing the newly planted crop.</summary>
    public GameObject CropInstance;

    /// <summary>Tilemap cell coordinate where the crop was planted.</summary>
    public Vector3Int CellPos;

    /// <summary>Crop configuration asset for this planting.</summary>
    public CropDataSO CropData;
}

/// <summary>
/// Raised when a crop advances its visual stage or reaches maturity on a morning growth tick.
/// </summary>
public struct OnCropStageChangedEvent : IEvent
{
    /// <summary>The root GameObject of the crop that progressed.</summary>
    public GameObject CropInstance;

    /// <summary>Tilemap cell coordinate of the crop.</summary>
    public Vector3Int CellPos;

    /// <summary>Growth stage index before this advancement.</summary>
    public int PreviousStageIndex;

    /// <summary>Growth stage index after this advancement.</summary>
    public int NewStageIndex;

    /// <summary>True when the crop has entered the Mature state after this advancement.</summary>
    public bool IsMature;
}

/// <summary>
/// Raised when a crop withers due to an out-of-season rollover or prolonged neglect.
/// </summary>
public struct OnCropWitheredEvent : IEvent
{
    /// <summary>The root GameObject of the withered crop.</summary>
    public GameObject CropInstance;

    /// <summary>Tilemap cell coordinate of the withered crop.</summary>
    public Vector3Int CellPos;

    /// <summary>Crop configuration asset identifying which crop withered.</summary>
    public CropDataSO CropData;
}

/// <summary>
/// Raised when the player triggers a harvest interaction on a mature crop.
/// </summary>
public struct OnCropHarvestRequestedEvent : IEvent
{
    /// <summary>The root GameObject of the crop being harvested.</summary>
    public GameObject CropInstance;

    /// <summary>Tilemap cell coordinate of the crop being harvested.</summary>
    public Vector3Int CellPos;

    /// <summary>The player or tool GameObject that initiated the harvest.</summary>
    public GameObject Interactor;
}

/// <summary>
/// Raised after daily crop growth evaluation when soil hydration resets to dry for the new day.
/// </summary>
public struct OnSoilHydrationResetEvent : IEvent
{
    /// <summary>Number of soil tiles whose hydration state was reset to dry.</summary>
    public int ResetTilesCount;
}

#endregion

#region Time & Calendar Events

/// <summary>
/// Raised on every minute tick. Drives all time-sensitive systems (lighting, HUD, etc.).
/// </summary>
public struct OnTimeTickEvent : IEvent
{
    public GameDateTime CurrentDateTime;
    public float        NormalizedDayTime;
}

/// <summary>
/// Raised once when the in-game hour value changes.
/// </summary>
public struct OnHourChangedEvent : IEvent
{
    public int          PreviousHour;
    public int          NewHour;
    public GameDateTime CurrentDateTime;
}

/// <summary>
/// Raised at the end of a day, just before the sleep transition begins.
/// </summary>
public struct OnDayEndedEvent : IEvent
{
    public GameDateTime EndedDateTime;
    /// <summary>True if the day ended because the player passed out rather than sleeping voluntarily.</summary>
    public bool         IsPassout;
}

/// <summary>
/// Raised when a new in-game day starts (after sleep or passout recovery).
/// </summary>
public struct OnNewDayStartedEvent : IEvent
{
    public GameDateTime NewDateTime;
    /// <summary>True if this new day follows a passout event.</summary>
    public bool         WasPassout;
}

/// <summary>
/// Raised when the current season rolls over to the next one.
/// </summary>
public struct OnSeasonChangedEvent : IEvent
{
    public Season PreviousSeason;
    public Season NewSeason;
    public int    Year;
}

/// <summary>
/// Raised when the player is force-teleported to bed due to passing out.
/// </summary>
public struct OnPlayerPassedOutEvent : IEvent
{
    public GameDateTime PassoutTime;
    /// <summary>Fraction (0.0 – 1.0) of stamina to deduct as a penalty on the next morning.</summary>
    public float        StaminaPenaltyPercent;
}

/// <summary>
/// Raised whenever the time simulation is paused or unpaused.
/// </summary>
public struct OnTimePausedStateChangedEvent : IEvent
{
    public bool IsPaused;
}

#endregion
