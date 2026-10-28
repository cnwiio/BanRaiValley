# Task Overview & Completed Work

This file tracks all completed tasks performed by Coder Agents across the project. Other agents can read this file to understand the current implementation state, modified files, and recent system additions.

---

## Completed Tasks Summary Table

| Task ID / Name | System / Feature | Files Created / Modified | Completed Date |
| :--- | :--- | :--- | :--- |
| TASK-001: Shadow Stutter & Terrain Flickering Fix | Lighting & Time System / Scene Configuration | `Assets/Project/Scenes/BuildScene 2.unity`<br>`Assets/Project/Scripts/Time/DayNightLightingController.cs`<br>`Assets/Project/Scripts/Time/Time Manager.cs`<br>`.agent/Manuals/ShadowConfigurationManual.md` | 2026-10-28 |
| TASK-002: Dirt Disappearance Over Inactivity | Farming System | `Assets/Project/Scripts/Farming/Dirt.cs`<br>`Assets/Project/Scripts/Farming/Dirt Manager.cs` | 2026-10-28 |
| TASK-003: Sprint and Crouch System | Player Movement System | `Assets/Project/Scripts/Player/Player Movement Data.cs`<br>`Assets/Project/Data/PlayerMovementData.asset`<br>`Assets/Project/Scripts/Player/Player Input Reader.cs`<br>`Assets/Project/Scripts/Player/Player Movement.cs`<br>`Assets/Project/Prefabs/Player.prefab` | 2026-10-28 |

---

## Detailed Task Changelog

### TASK-003: Sprint and Crouch System
- **Feature & Requirements**:
  1. Hold to Sprint (Shift) and Hold to Crouch (Ctrl).
  2. Crouch takes precedence over sprint (cannot sprint while crouching; jumping while crouching is disabled).
  3. Sprinting allowed in any movement direction.
  4. Smooth camera / head lowering and CharacterController collider height adjustment with feet grounded.
  5. Overhead clearance check preventing the player from standing up while under low ceilings.
- **Solution**:
  1. Extended `PlayerMovementData.cs` and `PlayerMovementData.asset` with `SprintSpeed` (10f), `CrouchSpeed` (3f), `CrouchHeight` (1.0f), `CrouchHeadY` (0.25f), and `CrouchTransitionSpeed` (12f).
  2. Updated `PlayerInputReader.cs` to bind `Run` and `Crouch` input actions (with fallback asset resolution if unassigned in inspector) and exposed `IsSprintPressed` and `IsCrouchPressed`.
  3. Updated `PlayerMovement.cs`:
     - Added `UpdateCrouchState()` with smooth exponential damping for `CharacterController.height`, `CharacterController.center`, and `Head.localPosition.y`.
     - Implemented `HasCeilingObstacle()` via `Physics.SphereCast` upward to enforce crouch until clearance is restored.
     - Updated `HandleHorizontalMovement()` to handle speed selection: Crouch (3) > Sprint (10) > Walk (6).
     - Guarded `HandleJumpInput()` to block jumping while crouching.
  4. Wired `RunAction` and `CrouchAction` in `Assets/Project/Prefabs/Player.prefab`.
  5. Refreshed knowledge graph via `graphify update .`.

### TASK-002: Dirt Disappearance Over Inactivity
- **Problem**:
  1. Tilled dirt tiles remained indefinitely, even when unwatered or unplanted for multiple days.
  2. Need dirt to decay and disappear if not interacted with for `x` days (resetting counter when watered or planted, advancing counter on new day, and disappearing on new day).
- **Solution**:
  1. Added `InactiveDays` tracking to `Dirt.cs` (`IncrementInactiveDays()`, `ResetInactiveDays()`). Initialized on spawn and reset on watering.
  2. In `DirtManager.cs`, added `daysToDisappear` (default 3) serialized field.
  3. Subscribed `DirtManager` to `OnPlantingEvent` to reset the dirt tile's inactivity counter when a seed is planted.
  4. On `OnNewDayStartedEvent`, evaluated all tracked dirt tiles:
     - If the tile has an active plant (`_grid.IsPlanted(...)`), it is protected from decay and its inactivity count resets.
     - If empty, increment `InactiveDays`. If `InactiveDays >= daysToDisappear`, untill the cell in `IFarmingGrid` (`TryUntill`), despawn the dirt prefab back to `LeanPool`, and unregister it from tracking.
     - Surviving tiles proceed with regular new-day unwatering visual and state resets.
  5. Updated knowledge graph via `graphify update .`.

### TASK-001: Shadow Stutter & Terrain Flickering Fix
- **Problem**:
  1. Shadows violently flickered and shifted out of place when moving the camera or walking due to `Terrain (1)` having `ShadowCastingMode = TwoSided` (value 2) with heightmap LOD chunk culling.
  2. Stationary shadows hitched every 2.5 seconds (10 in-game minutes) because `TimeManager` only emitted discrete tick events, which drove a fast exponential slerp in `DayNightLightingController` that completed in 0.3s and froze for 2.2s.
- **Solution**:
  1. Disabled terrain shadow casting (`m_ShadowCastingMode: 0`) on `Terrain (1)` and `Terrain` in `BuildScene 2.unity`. The terrain still receives shadows normally from all trees, houses, fences, and player.
  2. Exposed `ContinuousNormalizedDayTime` in `TimeManager.cs` to calculate sub-tick progression and updated `DayNightLightingController.cs` to rotate the celestial sun smoothly per render frame in `UpdateContinuousLighting()`.
  3. Kept the HUD clock and calendar progression discrete (updating on 10-minute intervals).
  4. Created `.agent/Manuals/ShadowConfigurationManual.md` detailing Unity Editor settings.
