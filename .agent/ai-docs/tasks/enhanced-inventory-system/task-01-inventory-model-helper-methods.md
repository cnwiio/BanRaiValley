# Task 01: Inventory Model Helper Methods

## 1. Task Goal
Extend `SlotData`, `IInventory`, and `BaseInventory` with atomic slot manipulation methods (taking stacks, taking half stacks, taking single items, depositing single items, and auto-stashing) required for Minecraft-style inventory interactions.

## 2. Task Information
- **System**: Enhanced Inventory System
- **Parent Plan**: [.agent/ai-docs/plan/enhanced-inventory-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/enhanced-inventory-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Slot Data.cs`
  - `Assets/Project/Scripts/Inventory/IInventory.cs`
  - `Assets/Project/Scripts/Inventory/Base Inventory.cs`
- **Dependencies / Prerequisites**:
  - Existing `SlotData` struct and `BaseInventory` class.
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `SlotData` in `Assets/Project/Scripts/Inventory/Slot Data.cs`**:
   - Add property `AvailableSpace => item != null && item.stackable ? item.MaxStack - count : 0;`.
   - Add method `public SlotData Split(int amountToTake)`:
     - Clamps `amountToTake` between 1 and `count`.
     - Decrements `count` by `amountToTake`.
     - Returns a new `SlotData` with `item` and `count = amountToTake`.
     - If remaining `count <= 0`, calls `Clear()`.

2. **Update `IInventory` interface in `Assets/Project/Scripts/Inventory/IInventory.cs`**:
   - Declare the following method signatures:
     ```csharp
     SlotData TakeStack(int index);
     SlotData TakeHalfStack(int index);
     SlotData TakeSingleItem(int index);
     bool AddSingleItemToSlot(int index, Item item);
     bool AutoStashItem(SlotData data, int preferredIndex = -1);
     ```

3. **Implement Methods in `BaseInventory` in `Assets/Project/Scripts/Inventory/Base Inventory.cs`**:
   - `public virtual SlotData TakeStack(int index)`:
     - Check `IsValidIndex(index)`. If invalid or empty, return default.
     - Cache `SlotData taken = inventorySlots[index];`
     - Call `ClearSlot(index);`
     - Return `taken`.
   - `public virtual SlotData TakeHalfStack(int index)`:
     - Check `IsValidIndex(index)`. If invalid or empty, return default.
     - Calculate `int halfCount = Mathf.CeilToInt(inventorySlots[index].count / 2f);`
     - Return `inventorySlots[index].Split(halfCount)`.
   - `public virtual SlotData TakeSingleItem(int index)`:
     - Check `IsValidIndex(index)`. If invalid or empty, return default.
     - Return `inventorySlots[index].Split(1)`.
   - `public virtual bool AddSingleItemToSlot(int index, Item item)`:
     - If `!IsValidIndex(index) || item == null` return false.
     - If `inventorySlots[index].IsEmpty`: set `inventorySlots[index] = new SlotData { item = item, count = 1 }; return true;`
     - If `inventorySlots[index].item == item && item.stackable && inventorySlots[index].count < item.MaxStack`:
       - `inventorySlots[index].count++; return true;`
     - Otherwise return false.
   - `public virtual bool AutoStashItem(SlotData data, int preferredIndex = -1)`:
     - If `data.IsEmpty` return true.
     - If `preferredIndex >= 0 && IsValidIndex(preferredIndex)`:
       - If `inventorySlots[preferredIndex].IsEmpty`: set `inventorySlots[preferredIndex] = data; return true;`
       - If `inventorySlots[preferredIndex].item == data.item && data.item.stackable`:
         - Add to stack. If all absorbed, return true; if remaining, proceed to add remainder.
     - If `CanAddItem(data.item, data.count)`:
       - `AddItem(data.item, data.count); return true;`
     - Return false (overflow / no space).

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] All methods validate slot indices using `IsValidIndex(index)`.
- [ ] `TakeHalfStack` rounds up (`CeilToInt`), leaving the smaller remainder in the slot.
- [ ] `AutoStashItem` prioritizes the `preferredIndex` before searching other inventory slots.
- [ ] All private fields follow `_camelCase` naming conventions.
