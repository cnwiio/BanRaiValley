# Task 08: Tool Subsystem Documentation & User Manual

## 1. Task Goal
Create `Assets/Project/Scripts/Farming/Tool/README.md` following Rule 16 of the architecture guide to provide an overview of the Tool Subsystem and a complete user manual for designers and developers on creating and tuning upgraded tools.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Tool/README.md` [NEW]
- **Dependencies / Prerequisites**:
  - Tasks 01 through 07 completed.
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 16: Mandatory folder README with Overview and User Manual)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `Assets/Project/Scripts/Farming/Tool/README.md`**:
   - Provide an **Overview**:
     - Role of `FarmingToolBase`, `Hoe`, `WateringCan`, `SeedBag`.
     - How `ToolAreaCalculator` handles multi-cell math (cardinal facing line and centered NxN square).
     - How `PlacementPreviewer` displays multi-cell holograms using `LeanPool`.
     - EventBus decoupling (`OnTillingImpactEvent`, `OnWateringEvent`, `MultiPreviewingEvent`).
   - Provide a **User Manual for Designers**:
     - Step-by-step instructions on creating a new upgraded tool Item (e.g. Copper Hoe or Gold Watering Can) in Unity:
       1. Create or select an `Item` ScriptableObject in `Assets/Project/Data/Items/` (or equivalent).
       2. Set `ItemType` to `Tool`.
       3. Assign the visual model/prefab and hotbar icon.
       4. Under **Tool Upgrade Data**:
          - Set `toolType` (`Hoe` or `WateringCan`).
          - Set `tier` (`Basic`, `Copper`, `Silver`, `Gold`, `Iridium`).
          - Set `patternShape`:
            - `ForwardLine` for Hoe.
            - `CenteredSquare` for Watering Can.
          - Set `patternDimension`:
            - Hoe: `1` (1 tile), `3` (3 tiles forward), `5` (5 tiles forward).
            - Watering Can: `1` (1x1), `3` (3x3), `5` (5x5).
     - How to test in Scene View and Play Mode.
   - Include a **Component Reference Table** detailing all classes, interfaces, and events.

## 4. Verification & Testing Checklist
- [ ] `README.md` is created in `Assets/Project/Scripts/Farming/Tool/`.
- [ ] Contains both Overview and step-by-step User Manual sections.
- [ ] Markdown links and tables format cleanly.
