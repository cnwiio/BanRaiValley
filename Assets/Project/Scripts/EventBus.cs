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
