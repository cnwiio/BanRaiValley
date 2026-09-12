# Task 02: Slot Interaction Events & UI Click Handling

## 1. Task Goal
Define unified UI slot click events in `EventBus.cs` and implement `IPointerClickHandler` on `InventorySlotUI.cs` to capture Left/Right clicks with Shift/Ctrl modifiers, while updating stack count display for all stackable items.

## 2. Task Information
- **System**: Enhanced Inventory System
- **Parent Plan**: [.agent/ai-docs/plan/enhanced-inventory-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/enhanced-inventory-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/EventBus.cs`
  - `Assets/Project/Scripts/Inventory/Inventory Slot UI.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`IInventory` methods)
  - `UnityEngine.InputSystem` Keyboard status
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Define Events in `Assets/Project/Scripts/EventBus.cs`**:
   - In `#region Inventory UI`, declare:
     ```csharp
     public struct OnUISlotClickEvent : IEvent
     {
         public int Index;
         public IInventory Inventory;
         public InventorySlotUI SlotUI;
         public UnityEngine.EventSystems.PointerEventData.InputButton Button;
         public bool IsShiftPressed;
         public bool IsCtrlPressed;
     }

     public struct OnUIBackgroundClickEvent : IEvent { }

     public struct InventoryValidateHeldItemEvent : IEvent { }
     ```

2. **Update `InventorySlotUI.cs` Interfaces & Implementation**:
   - Add `IPointerClickHandler` to `InventorySlotUI` class definition:
     ```csharp
     public class InventorySlotUI : MonoBehaviour, IPoolable, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
     ```
   - Implement `public void OnPointerClick(PointerEventData eventData)`:
     - Check if `eventData.dragging` is true; if dragging, return to avoid conflicting with drag-and-drop.
     - Check modifier keys safely using `UnityEngine.InputSystem.Keyboard.current`:
       ```csharp
       bool isShift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
       bool isCtrl = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
       ```
     - Raise event:
       ```csharp
       EventBus<OnUISlotClickEvent>.Raise(new OnUISlotClickEvent
       {
           Index = SlotIndex,
           Inventory = inventoryModel,
           SlotUI = this,
           Button = eventData.button,
           IsShiftPressed = isShift,
           IsCtrlPressed = isCtrl
       });
       ```

3. **Update Visual Stack Count in `InventorySlotUI.RenderVisual()`**:
   - Replace the legacy seed-only count check (`if (SlotData.item.type == ItemType.Seed)`) with:
     ```csharp
     if (SlotData.item.stackable && count > 1)
     {
         countText.SetText($"{count}");
         countText.enabled = true;
     }
     else
     {
         countText.enabled = false;
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] `IPointerClickHandler` detects Left and Right clicks without interfering with `OnBeginDrag` / `OnDrop`.
- [ ] Stack count text displays accurately for all stackable items when count > 1, and hides when count is 1.
- [ ] `EventBus<InventoryUIRefreshEvent>` continues to update slot visuals cleanly.
