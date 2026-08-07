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

public struct OnAction1Event : IEvent { }
public struct OnAction2Event : IEvent { }

public struct OnHoeDoAction1Event : IEvent 
{
    public Vector3 Position;
}

public struct OnHoeDoAction2Event : IEvent
{
}

public struct OnHoeFarmingMode : IEvent
{
    public Vector3 Position;
}