# Task Overview & Completed Work

This file tracks all completed tasks performed by Coder Agents across the project. Other agents can read this file to understand the current implementation state, modified files, and recent system additions.

---

## Completed Tasks Summary Table

| Task ID / Name | System / Feature | Files Created / Modified | Completed Date |
| :--- | :--- | :--- | :--- |
| TASK-001: Shadow Stutter & Terrain Flickering Fix | Lighting & Time System / Scene Configuration | `Assets/Project/Scenes/BuildScene 2.unity`<br>`Assets/Project/Scripts/Time/DayNightLightingController.cs`<br>`Assets/Project/Scripts/Time/Time Manager.cs`<br>`.agent/Manuals/ShadowConfigurationManual.md` | 2026-10-28 |
| TASK-002: Dirt Disappearance Over Inactivity | Farming System | `Assets/Project/Scripts/Farming/Dirt.cs`<br>`Assets/Project/Scripts/Farming/Dirt Manager.cs` | 2026-10-28 |
| TASK-003: Sprint and Crouch System | Player Movement System | `Assets/Project/Scripts/Player/Player Movement Data.cs`<br>`Assets/Project/Data/PlayerMovementData.asset`<br>`Assets/Project/Scripts/Player/Player Input Reader.cs`<br>`Assets/Project/Scripts/Player/Player Movement.cs`<br>`Assets/Project/Prefabs/Player.prefab` | 2026-10-28 |
| TASK-004: Continuous Planting and Harvesting System | Farming & Player Interaction | `Assets/Project/Scripts/EventBus.cs`<br>`Assets/Project/Scripts/Player/Player Input Reader.cs`<br>`Assets/Project/Scripts/Player/Player Interactor.cs`<br>`Assets/Project/Scripts/Farming/Tool/Seed Bag.cs`<br>`Assets/Project/Animation/Seed Bag/Planting.anim`<br>`Assets/Project/Animation/Seed Bag/Seed bag.controller`<br>`Assets/Project/Editor/BanRaiValleyEditorCLI.cs` | 2026-10-28 |
| TASK-005: Continuous Attack System (Bare Hand & Hoe) | Combat & Attackable Tools | `Assets/Project/Scripts/Combat/ComboAttackController.cs`<br>`Assets/Project/Scripts/Bare Hand.cs`<br>`Assets/Project/Scripts/Farming/Tool/Hoe.cs`<br>`Assets/Project/Scripts/Player/Player Interactor.cs` | 2026-10-28 |

---

## Detailed Task Changelog

### TASK-005: Continuous Attack System (Bare Hand & Hoe)
- **Feature & Requirements**:
  1. Make attackable tools (`BareHand` and `Hoe`) holdable with continuous combo attacking.
  2. Holding Left Mouse Button (`Primary Action`) automatically cycles combo attacks seamlessly (Attack 1 -> Attack 2 for Bare Hand; Attack 1 -> Attack 2 -> Attack 3 for Hoe).
  3. Single clicks still function as discrete queued attacks without double-attacking.
  4. Exposed configurable `holdThreshold` variable in Inspector on both `BareHand` and `Hoe`.
  5. Movement and sprinting remain at full freedom while attacking.
  6. Hoe's Farming/Tilling/Deleting modes and right-click toggle remain completely unaffected.
- **Root Cause of Single-Click Double-Attacking**:
  1. Unity Input System fires `started` and `performed` back-to-back on button press down. `started` called `SetHoldAttack(true)` / `Attack()`, setting `isAttacking = true`. Immediately on the same frame, `performed` fired `OnPrimaryActionEvent`, which called `TryAttack()`. Because `isAttacking` was already true, it double-buffered `pendingAttack = true`, causing an unwanted second swing when the first swing finished.
  2. `BareHand`'s animation hit event (`OnAnimationHit`) occurred at 0.166s, which is faster than an average human mouse click release (~0.18-0.22s). Without a hold duration threshold check, any normal tap was still physically held down when the first swing hit.
- **Solution**:
  1. Added exposed `[SerializeField] private float holdThreshold = 0.25f;` to both `BareHand` and `Hoe` (with Inspector tooltip).
  2. Extended `ComboAttackController.cs`:
     - Added `pressStartTime`, `HoldThreshold`, and dedicated `OnPressStarted()` / `OnPressCanceled()` methods.
     - In `OnAnimationHit()`, evaluates `bool isHoldActive = isHoldingAttack && (Time.time - pressStartTime >= holdThreshold);`. Only triggers continuous follow-up if hold threshold has elapsed.
     - In `Update()`, checks if the button is still held past `holdThreshold` to start the next combo swing if the initial swing finished before threshold.
  3. In `BareHand.cs`:
     - Removed redundant `OnPrimaryActionEvent` listener to eliminate phantom double-buffering.
     - Wired `OnPrimaryActionStarted` $\to$ `combo.OnPressStarted()`, `OnPrimaryActionCanceled` $\to$ `combo.OnPressCanceled()`.
     - Added `combo.HoldThreshold = holdThreshold; combo.Update();` in `Update()`.
  4. In `Hoe.cs`:
     - In `PrimaryAction()`, bypassed combo calls when in `HoeState.Idle` / `HoeState.Attacking` since press is already handled by `OnPrimaryActionStarted`.
     - In `Update()`, runs `combo.Update()` and dynamically updates `combo.HoldThreshold`.
     - Releasing button or changing action map cleanly cancels hold.
  5. Hardened `PlayerInteractor.cs`:
     - Added null safety checks for `Camera.main` and `Mouse.current` in `RayCastAtCursor()` to eliminate any startup/domain reload `NullReferenceException`.
  6. Verified compilation via Unity Editor log (0 errors) and updated knowledge graph via `graphify update .`.

### TASK-004: Continuous Planting and Harvesting System
- **Feature & Requirements**:
  1. Hold Left Mouse Button (`Primary Action`) with Seed Bag equipped to continuously plant seeds across tilled plots.
  2. Sowing animation loops smoothly while held and cleanly returns to Idle when released or when seeds run out.
  3. Hold `E` (`Interact`) to continuously harvest/awaken ready and withered crops targeted by the crosshair with a fast interval (~0.15s), avoiding click spam.
  4. Unity CLI access and batchmode verification utility.
- **Solution**:
  1. Extended `EventBus.cs` with `OnPrimaryActionStartedEvent`, `OnPrimaryActionCanceledEvent`, `OnInteractActionStartedEvent`, and `OnInteractActionCanceledEvent`.
  2. Updated `PlayerInputReader.cs` to track `IsPrimaryActionPressed` and `IsInteractPressed`, binding action `started` and `canceled` events.
  3. Updated `PlayerInteractor.cs` with hold state and timer (`INTERACT_INTERVAL = 0.15f`) to automatically trigger `Interact()` when sweeping over or holding on `IInteractable` targets.
  4. Updated `SeedBag.cs` with continuous planting loop (`PLANT_SWEEP_INTERVAL = 0.18f`), dynamically planting as the player sweeps over valid unplanted tilled tiles, and stopping cleanly when LMB is released or seeds run out.
  5. Set `m_LoopTime: 1` in `Planting.anim` and added `IsPlanting` bool parameter with transitions in `Seed bag.controller` for looping animation.
  6. Created `BanRaiValleyEditorCLI.cs` under `Assets/Project/Editor/` with static method `VerifyProject()` for headless Unity batchmode execution (`-executeMethod BanRaiValley.EditorCLI.VerifyProject`).
  7. Verified clean compilation via Unity Editor log (0 errors) and updated knowledge graph via `graphify update .`.
  8. Fixed hotbar selection lockup: In `InputManager.cs`, stopped disabling `HotbarActionMap` during continuous planting, added `OnEndPlantingEvent`, and guaranteed hotbar action map is always restored when planting ends.

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
