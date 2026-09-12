# Task 04: UI Lifecycle Validation & Background Auto-Stash

## 1. Task Goal
Safeguard against item loss during inventory interactions by validating floating cursor items upon inventory UI toggle/close, and implementing an empty background click listener that safely returns held items back to inventory.

## 2. Task Information
- **System**: Enhanced Inventory System
- **Parent Plan**: [.agent/ai-docs/plan/enhanced-inventory-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/enhanced-inventory-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Inventory UI.cs`
  - `Assets/Project/Scripts/Inventory/Inventory Background Click Detector.cs`
- **Dependencies / Prerequisites**:
  - Task 02 (`InventoryValidateHeldItemEvent`, `OnUIBackgroundClickEvent`)
  - Task 03 (`InventoryUIController.ReturnHeldItemToInventory()`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `InventoryUI.cs` Lifecycle Validation**:
   - In `Assets/Project/Scripts/Inventory/Inventory UI.cs`:
     - In `ToggleInventoryUI()`:
       - Before closing the UI (when `IsInventoryUIActive` is true, i.e., about to close):
         ```csharp
         if (IsInventoryUIActive)
         {
             EventBus<InventoryValidateHeldItemEvent>.Raise(new InventoryValidateHeldItemEvent());
         }
         ```
       - This guarantees that `InventoryUIController` stashes any active cursor-held items before `LeanPool.Despawn` runs on the slot UIs.

2. **Create `InventoryBackgroundClickDetector.cs`**:
   - Create new file `Assets/Project/Scripts/Inventory/Inventory Background Click Detector.cs`:
     ```csharp
     using UnityEngine;
     using UnityEngine.EventSystems;

     public class InventoryBackgroundClickDetector : MonoBehaviour, IPointerClickHandler
     {
         public void OnPointerClick(PointerEventData eventData)
         {
             EventBus<OnUIBackgroundClickEvent>.Raise(new OnUIBackgroundClickEvent());
         }
     }
     ```
   - Attach this component to the root background panels of the Inventory UI canvas (or transparent backdrop) so clicks anywhere outside slot buttons trigger the auto-stash return.

3. **Verify Edge Cases**:
   - Validate that toggling inventory with keyboard shortcut while holding an item does not destroy the item or leave an orphan floating cursor image.
   - Validate that clicking on empty backdrop returns item back to original slot without errors.

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Closing inventory with an item attached to the cursor safely stashes it back into the player's inventory without item duplication or deletion.
- [ ] Clicking on empty UI background space immediately returns the held item to the origin slot.
- [ ] Cursor state and action maps correctly reset upon inventory close.
