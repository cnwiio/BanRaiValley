# Task 01: Tool Data Models & Item Integration

## 1. Task Goal
Define pure data structures, enums, and an injection interface to support tool progression and area-of-effect parameters, and expose them on the [Item.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/Item.cs) ScriptableObject.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/ItemToolData.cs` [NEW]
  - `Assets/Project/Scripts/Inventory/IToolItemReceiver.cs` [NEW]
  - `Assets/Project/Scripts/Inventory/Item.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Existing [Item.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/Item.cs) and [ItemAttackData.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Inventory/ItemAttackData.cs).
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (ScriptableObject = Data only, no runtime mutation)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Standard casing, serializable structs)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `ItemToolData.cs`**:
   - Create new file at `Assets/Project/Scripts/Inventory/ItemToolData.cs`.
   - Define enum `ToolType`:
     ```csharp
     public enum ToolType
     {
         None = 0,
         Hoe = 1,
         WateringCan = 2,
         Axe = 3,
         Pickaxe = 4
     }
     ```
   - Define enum `ToolTier`:
     ```csharp
     public enum ToolTier
     {
         Basic = 0,
         Copper = 1,
         Silver = 2,
         Gold = 3,
         Iridium = 4
     }
     ```
   - Define enum `ToolAreaPattern`:
     ```csharp
     public enum ToolAreaPattern
     {
         Single = 0,
         ForwardLine = 1,
         CenteredSquare = 2
     }
     ```
   - Define `[System.Serializable] public struct ItemToolData`:
     - `public ToolType toolType;`
     - `public ToolTier tier;`
     - `public ToolAreaPattern patternShape;`
     - `[Tooltip("Dimension of the pattern (e.g. 1 for 1 tile, 3 for 3-tile line or 3x3 square, 5 for 5-tile line or 5x5 square)")]`
     - `public int patternDimension;`
     - `public float staminaCost;`
     - Static default factory property `DefaultHoe`: returns `ItemToolData` with `ToolType.Hoe`, `ToolTier.Basic`, `ToolAreaPattern.ForwardLine`, `patternDimension = 1`, `staminaCost = 1f`.
     - Static default factory property `DefaultWateringCan`: returns `ItemToolData` with `ToolType.WateringCan`, `ToolTier.Basic`, `ToolAreaPattern.CenteredSquare`, `patternDimension = 1`, `staminaCost = 1f`.

2. **Create `IToolItemReceiver.cs`**:
   - Create new file at `Assets/Project/Scripts/Inventory/IToolItemReceiver.cs`.
   - Define interface:
     ```csharp
     public interface IToolItemReceiver
     {
         void BindItemData(Item item);
     }
     ```

3. **Update `Item.cs`**:
   - In `Assets/Project/Scripts/Inventory/Item.cs`, add serialized field under `[Header("Tool Upgrade Data")]`:
     ```csharp
     [Header("Tool Upgrade Data")]
     [Tooltip("Tool upgrade statistics, pattern shapes, and tier configurations.")]
     [SerializeField] private ItemToolData _toolData = ItemToolData.DefaultHoe;

     /// <summary>Tool upgrade parameters defined for this item.</summary>
     public ItemToolData ToolData => _toolData;
     ```

## 4. Verification & Testing Checklist
- [ ] `ItemToolData.cs` and `IToolItemReceiver.cs` compile cleanly in Unity 6.3.
- [ ] Opening any `Item` asset in the Unity Inspector displays the new **Tool Upgrade Data** foldout/header with enum dropdowns and dimension fields.
- [ ] Existing `Item` assets retain backwards compatibility without serialization errors.
