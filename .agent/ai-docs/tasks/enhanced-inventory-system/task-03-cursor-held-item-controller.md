# Task 03: Minecraft-Style Cursor Held-Item Controller

## 1. Task Goal
Refactor `InventoryUIController.cs` to manage floating cursor held items, processing Left-Click (pickup/place/swap), Right-Click (split half/place single), Ctrl-Click (take single/increment), and Shift-Click (bidirectional quick transfer) while maintaining backwards compatibility with Drag and Drop.

## 2. Task Information
- **System**: Enhanced Inventory System
- **Parent Plan**: [.agent/ai-docs/plan/enhanced-inventory-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/enhanced-inventory-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Inventory UI Controller.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`IInventory` atomic methods)
  - Task 02 (`OnUISlotClickEvent`, `OnUIBackgroundClickEvent`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **State & Serialized References in `InventoryUIController.cs`**:
   - Add serialized references for inventories to enable bidirectional transfer:
     ```csharp
     [Header("Inventory References")]
     [SerializeField] private InventoyModel _mainInventory;
     [SerializeField] private HotbarInventoryModel _hotbarInventory;
     ```
   - Add held-item state variables:
     ```csharp
     private SlotData _heldSlotData;
     private IInventory _originInventory;
     private int _originSlotIndex = -1;
     private bool HasHeldItem => !_heldSlotData.IsEmpty;
     private Coroutine _cursorFollowCoroutine;
     ```

2. **Event Subscriptions**:
   - In `OnEnable()`:
     - Subscribe to `OnUISlotClickEvent`, `OnUIBackgroundClickEvent`, and `InventoryValidateHeldItemEvent`.
   - In `OnDisable()`:
     - Unsubscribe from all above events.
     - Ensure any active coroutine is stopped.

3. **Cursor Floating Icon Coroutine**:
   - When `_heldSlotData` becomes non-empty, start `UpdateCursorFollowCoroutine()`:
     ```csharp
     private IEnumerator UpdateCursorFollowCoroutine()
     {
         while (HasHeldItem)
         {
             if (Mouse.current != null)
             {
                 DragTransform.position = Mouse.current.position.ReadValue();
             }
             yield return null;
         }
     }
     ```
   - Update `EnableDragIcon(SlotData slot)` to show sprite and stack count if `slot.count > 1`.

4. **Slot Click Dispatcher**:
   - Implement `void OnSlotClicked(OnUISlotClickEvent evt)`:
     ```csharp
     if (evt.IsShiftPressed)
     {
         HandleShiftClick(evt);
     }
     else if (evt.IsCtrlPressed)
     {
         HandleCtrlClick(evt);
     }
     else if (evt.Button == PointerEventData.InputButton.Left)
     {
         HandleLeftClick(evt);
     }
     else if (evt.Button == PointerEventData.InputButton.Right)
     {
         HandleRightClick(evt);
     }
     EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());
     ```

5. **Interaction Handlers**:
   - **`HandleLeftClick(OnUISlotClickEvent evt)`**:
     - *If `!HasHeldItem`*: Pick up stack from slot via `evt.Inventory.TakeStack(evt.Index)`. Store in `_heldSlotData`, remember `_originInventory = evt.Inventory` and `_originSlotIndex = evt.Index`. Enable floating icon.
     - *If `HasHeldItem`*:
       - Slot empty: Place held item into slot via `evt.Inventory.SetSlotData(_heldSlotData, evt.Index)`. Call `ClearHeldItem()`.
       - Slot has same item (`slot.item == _heldSlotData.item` and `stackable`):
         - `int overflow = evt.Inventory.AddStackItemToSlot(evt.Index, _heldSlotData.item, _heldSlotData.count);`
         - If `overflow > 0`: `_heldSlotData.count = overflow; UpdateDragIcon(_heldSlotData);`
         - Else: `ClearHeldItem();`
       - Slot has different item: **Swap**!
         - `SlotData slotItem = evt.Inventory.GetSlotData(evt.Index);`
         - `evt.Inventory.SetSlotData(_heldSlotData, evt.Index);`
         - `_heldSlotData = slotItem;`
         - `_originInventory = evt.Inventory; _originSlotIndex = evt.Index;`
         - `UpdateDragIcon(_heldSlotData);`
   - **`HandleRightClick(OnUISlotClickEvent evt)`**:
     - *If `!HasHeldItem`*: Take half stack from slot via `evt.Inventory.TakeHalfStack(evt.Index)`. Set `_originInventory` and `_originSlotIndex`. Enable floating icon.
     - *If `HasHeldItem`*:
       - Try placing 1 item via `evt.Inventory.AddSingleItemToSlot(evt.Index, _heldSlotData.item)`.
       - If placed: decrement `_heldSlotData.count`. If `<= 0`, call `ClearHeldItem()`, else `UpdateDragIcon(_heldSlotData)`.
   - **`HandleCtrlClick(OnUISlotClickEvent evt)`**:
     - *If `!HasHeldItem`*: Take single item via `evt.Inventory.TakeSingleItem(evt.Index)`. Enable floating icon.
     - *If `HasHeldItem`*: If slot has same item and `_heldSlotData.count < _heldSlotData.item.MaxStack`, take 1 from slot and add to `_heldSlotData.count`.
   - **`HandleShiftClick(OnUISlotClickEvent evt)`**:
     - Target inventory determination:
       - If `evt.Inventory == (IInventory)_mainInventory`, target is `_hotbarInventory`.
       - If `evt.Inventory == (IInventory)_hotbarInventory`, target is `_mainInventory`.
     - Read slot data at `evt.Index`. If empty, return.
     - Attempt `target.TryAddItem(data.item, data.count)`. If successful, `evt.Inventory.ClearSlot(evt.Index)`.

6. **Safety & Drag-and-Drop Compatibility**:
   - In `OnBeginDrag`: If `HasHeldItem`, abort drag.
   - Implement `ReturnHeldItemToInventory()`:
     ```csharp
     public void ReturnHeldItemToInventory()
     {
         if (!HasHeldItem) return;
         bool stashed = _originInventory != null && _originInventory.AutoStashItem(_heldSlotData, _originSlotIndex);
         if (!stashed && _mainInventory != null)
         {
             stashed = _mainInventory.AutoStashItem(_heldSlotData);
         }
         if (!stashed && _hotbarInventory != null)
         {
             _hotbarInventory.AutoStashItem(_heldSlotData);
         }
         ClearHeldItem();
         EventBus<InventoryUIRefreshEvent>.Raise(new InventoryUIRefreshEvent());
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Left click picks up stack when cursor empty, places into empty slot, tops off matching stacks, and swaps on different items.
- [ ] Right click picks up half stack when cursor empty, and places 1 item per click when holding an item.
- [ ] Ctrl + Click takes 1 item from slot onto cursor.
- [ ] Shift + Click transfers items seamlessly between Main Inventory and Hotbar.
- [ ] Drag-and-drop continues to work seamlessly without conflict.
