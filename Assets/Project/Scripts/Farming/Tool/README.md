# Tool Subsystem & Upgrade Architecture

## Overview

The **Tool Subsystem** powers player interaction with the farming grid, supporting single-tile and multi-tile tool actions (such as upgraded Hoes and Watering Cans) with zero runtime GC allocation.

### Core Architecture & Key Components

1. **`FarmingToolBase`** (`FarmingToolBase.cs`):
   - Abstract skeleton for all farming tools. Handles mouse raycasting against the `IFarmingGrid`, input event wiring (`OnPrimaryActionEvent`, `OnSecondaryActionEvent`), and lazy grid resolution.
   - Implements `IToolItemReceiver` to receive `ItemToolData` whenever spawned by `PlayerHandVisualizer`.
   - Executes `RunMultiPreviewUpdate(...)` to evaluate multi-tile target footprints without runtime heap allocations.

2. **`ToolAreaCalculator`** (`ToolAreaCalculator.cs`):
   - Pure, stateless service class for grid pattern calculations.
   - `GetCardinalFacingDirection(Vector3)`: Projects 3D camera/player forward vectors onto the XZ plane and snaps to the nearest cardinal grid vector (+Z North, -Z South, +X East, -X West).
   - `CalculatePatternCells(...)`: Fills pre-allocated buffers for `Single`, `ForwardLine` (straight-line in facing direction), and `CenteredSquare` (NxN centered grid footprint).

3. **`PlacementPreviewer` & Multi-Hologram Pooling** (`Placement Previewer.cs`):
   - Listens to `MultiPreviewingEvent` on `EventBus`.
   - Uses `LeanPool` to dynamically scale an active hologram pool (`_activeHolograms`) for NxN or 1xN multi-tile previews.
   - Applies `IPreviewVisualStrategy` per cell (valid vs invalid materials) based on individual tile validity.

4. **Event-Driven Execution**:
   - `OnTillingImpactEvent`: Fired when a Hoe animation finishes impact, tilling all valid target tiles in the pending footprint simultaneously.
   - `OnWateringEvent`: Fired when a Watering Can animation completes, hydrating all valid target tiles in the pending footprint simultaneously.
   - `MultiPreviewingEvent`: Broadcasts cell world positions, per-cell validity, and rotation to `PlacementPreviewer`.

---

## Component & Event Reference

| Component / Contract | File Location | Responsibility |
| :--- | :--- | :--- |
| `ItemToolData` | `Assets/Project/Scripts/Inventory/ItemToolData.cs` | Struct defining `ToolType`, `ToolTier`, `ToolAreaPattern`, `patternDimension`, and `staminaCost`. |
| `IToolItemReceiver` | `Assets/Project/Scripts/Inventory/IToolItemReceiver.cs` | Interface for receiving tool data when an `Item` model is spawned in the player's hand. |
| `FarmingToolBase` | `Assets/Project/Scripts/Farming/Tool/FarmingToolBase.cs` | Abstract base class managing grid targeting, input, and multi-tile preview loops. |
| `ToolAreaCalculator` | `Assets/Project/Scripts/Farming/Tool/ToolAreaCalculator.cs` | Static math service calculating cardinal direction snapping and multi-cell pattern coordinates. |
| `Hoe` | `Assets/Project/Scripts/Farming/Tool/Hoe.cs` | Implements straight-line tilling in player facing direction with multi-tile holograms. |
| `WateringCan` | `Assets/Project/Scripts/Farming/Tool/Watering Can.cs` | Implements centered NxN square soil hydration with multi-tile holograms. |
| `PlacementPreviewer` | `Assets/Project/Scripts/Farming/Placement Previewer.cs` | Multi-hologram renderer using `LeanPool` and per-cell visual strategies. |
| `MultiPreviewingEvent` | `Assets/Project/Scripts/EventBus.cs` | Event payload carrying an array of `TilePreviewData` for multi-cell hologram rendering. |

---

## User Manual for Designers: Creating & Tuning Upgraded Tools

Follow these step-by-step instructions to create an upgraded tool item (e.g. Copper Hoe or Gold Watering Can) in Unity.

### 1. Create or Select an Item ScriptableObject
1. In the Unity Project window, navigate to `Assets/Project/Data/Items/` (or your project's item folder).
2. Right-click and choose **Create > Scriptable Objects > Item**.
3. Name your item asset (e.g. `Item_Hoe_Copper`, `Item_WateringCan_Gold`).

### 2. Configure Basic Item Properties
- **Type**: Set `ItemType` to `Tool`.
- **Image**: Assign the UI icon sprite for the item.
- **Prefab**: Assign the tool model prefab (containing the `Hoe` or `WateringCan` component).
- **Stackable**: Usually `false` (Max Stack = 1).

### 3. Configure Tool Upgrade Data
Expand the **Tool Upgrade Data** header in the Inspector:

#### For Upgraded Hoe (Straight-Line Tilling):
- **Tool Type**: `Hoe`
- **Tier**: Choose tier (`Basic`, `Copper`, `Silver`, `Gold`, `Iridium`).
- **Pattern Shape**: Select `ForwardLine`.
- **Pattern Dimension**:
  - `1`: Basic Hoe (1 tile).
  - `3`: Copper/Silver Hoe (3 tiles forward in player's cardinal facing direction).
  - `5`: Gold/Iridium Hoe (5 tiles forward).
- **Stamina Cost**: Set stamina deducted per swing (e.g., `1.0`).

#### For Upgraded Watering Can (Square Area Watering):
- **Tool Type**: `WateringCan`
- **Tier**: Choose tier (`Basic`, `Copper`, `Silver`, `Gold`, `Iridium`).
- **Pattern Shape**: Select `CenteredSquare`.
- **Pattern Dimension**:
  - `1`: Basic Watering Can (1x1 single tile).
  - `3`: Copper/Silver Watering Can (3x3 centered square = 9 tiles).
  - `5`: Gold/Iridium Watering Can (5x5 centered square = 25 tiles).
- **Stamina Cost**: Set stamina deducted per watering action.

---

## Testing & Verification Guidelines

1. **Equip Item**: Put the newly created `Item` asset in a hotbar slot or inventory.
2. **Select Hotbar Slot**: Switch to the item slot in-game. `PlayerHandVisualizer` will spawn the tool model and automatically inject its `ToolData` via `IToolItemReceiver`.
3. **Aim & Preview**:
   - For **Hoe**: Look around with the mouse. The hologram line will align to North (+Z), South (-Z), East (+X), or West (-X) based on camera view direction.
   - For **Watering Can**: Target any soil cell. A centered 3x3 or 5x5 grid hologram will highlight valid (green) and invalid/already-watered (red) tiles.
4. **Execute Action**:
   - Left-click to till or water. On animation impact/finish, all valid candidate tiles in the active pattern will be tilled/watered simultaneously.
