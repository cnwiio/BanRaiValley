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
| TASK-006: Plant AI Abstract Base Classes | Plant AI System | `Assets/Project/Scripts/AI/AIMovementBase.cs`<br>`Assets/Project/Scripts/AI/AIAttackBase.cs`<br>`Assets/Project/Scripts/AI/AIDetectionBase.cs`<br>`Assets/Project/Scripts/AI/AIAnimationControllerBase.cs`<br>`Assets/Project/Scripts/AI/AIHealthBase.cs` | 2026-10-28 |
| TASK-007: Plant AI Brain Base Coordinator | Plant AI System | `Assets/Project/Scripts/AI/AIBrainBase.cs` | 2026-10-28 |
| TASK-008: Concrete AI Components Inheritance Refactor | Plant AI System | `Assets/Project/Scripts/AI/AI Movement.cs`<br>`Assets/Project/Scripts/AI/AI Attack.cs`<br>`Assets/Project/Scripts/AI/AI Detection.cs`<br>`Assets/Project/Scripts/AI/AI Animation Controller.cs`<br>`Assets/Project/Scripts/AI/AI Health.cs` | 2026-10-28 |
| TASK-009: AI Brain Refactor to Inherit Base | Plant AI System | `Assets/Project/Scripts/AI/AI Brain.cs` | 2026-10-28 |
| TASK-010: Plant AI Architecture Documentation & Prefab Verification | Plant AI System | `Assets/Project/Scripts/AI/README.md`<br>`Assets/Project/Prefabs/Plant Monster/*.prefab` | 2026-10-28 |
| TASK-011: ESC Action Closes Active UI Stack & Cursor Auto-Lock Fix | UI & Input Management | `Assets/Project/Scripts/ICloseableUI.cs`<br>`Assets/Project/Scripts/UIStackManager.cs`<br>`Assets/Project/Scripts/Setting.cs`<br>`Assets/Project/Scripts/Inventory/Inventory UI.cs`<br>`Assets/Project/Scripts/Inventory/Inventory UI Controller.cs`<br>`Assets/Project/Scripts/Shop/ShopUIHandler.cs`<br>`Assets/Project/Scripts/Casino/Casino Interactor.cs`<br>`Assets/Project/Scripts/Player/Input Manager.cs` | 2026-10-28 |

---

## Detailed Task Changelog

### TASK-011: ESC Action Closes Active UI Stack (2026-10-28)
- **Target Files**:
  - [`Assets/Project/Scripts/ICloseableUI.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/ICloseableUI.cs) ([NEW])
  - [`Assets/Project/Scripts/UIStackManager.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/UIStackManager.cs) ([NEW])
  - [`Assets/Project/Scripts/Setting.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Setting.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/Inventory/Inventory UI.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Inventory/Inventory UI.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/Inventory/Inventory UI Controller.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Inventory/Inventory UI Controller.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/Shop/ShopUIHandler.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Shop/ShopUIHandler.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/Casino/Casino Interactor.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Casino/Casino Interactor.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/Player/Input Manager.cs`](file:///d:/Work/Unity Project/BanRaiValley/Assets/Project/Scripts/Player/Input Manager.cs) ([MODIFIED])
- **What Was Done**:
  - Created `ICloseableUI` contract specifying `CloseUI()` and `bool IsOpen`.
  - Implemented `UIStackManager` static LIFO manager that manages active UI panels and transitions action maps back to `ActionMapType.Player` when all panels are closed.
  - Updated `Setting.cs` to query `UIStackManager.TryCloseTopUI()` on `OnEscapeActionEvent`. When any UI is open, ESC closes the top-most window instead of overlaying the Pause/Settings menu. Only when no UI is open does ESC toggle the Pause menu.
  - Implemented `ICloseableUI` on `InventoryUI`, `ShopUIHandler`, `CasinoInteractor`, and `Setting`.
  - Added safe item auto-return in `InventoryUIController` (`ReturnHeldItem()`) to return any held/dragged stack back to its original slot/inventory upon closing via ESC or toggle.
  - **Fixed Cursor Lock & Movement Restoration**:
    - Centralized `ChangeActionMap` emission into `UIStackManager.RegisterOpen` and `UIStackManager.UnregisterClosed`, guaranteeing `ActionMapType.Player` is raised when any UI closes via non-ESC methods (e.g. inventory key 'I', close button click, interact trigger).
    - Added next-frame cursor lock re-assertion in `InputManager` to prevent Unity Editor ESC key releases and UGUI button mouse-up events from canceling cursor lock.
    - Added click-to-lock fallback in `InputManager.Update()` for window focus regaining in player mode.

### TASK-010: Plant AI Architecture Documentation & Prefab Verification (Task 05) — 2026-10-28 14:32
- **Target Files**:
  - [`Assets/Project/Scripts/AI/README.md`](../../Assets/Project/Scripts/AI/README.md) ([NEW])
  - [`Assets/Project/Prefabs/Plant Monster/Cabbage Monster.prefab`](../../Assets/Project/Prefabs/Plant%20Monster/Cabbage%20Monster.prefab) ([VERIFIED])
  - [`Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 1.prefab`](../../Assets/Project/Prefabs/Plant%20Monster/Cabbage%20Monster%20Tier%201.prefab) ([VERIFIED])
  - [`Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 2.prefab`](../../Assets/Project/Prefabs/Plant%20Monster/Cabbage%20Monster%20Tier%202.prefab) ([VERIFIED])
  - [`Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 3.prefab`](../../Assets/Project/Prefabs/Plant%20Monster/Cabbage%20Monster%20Tier%203.prefab) ([VERIFIED])
  - [`Assets/Project/Prefabs/Plant Monster/Test Plant Monster.prefab`](../../Assets/Project/Prefabs/Plant%20Monster/Test%20Plant%20Monster.prefab) ([VERIFIED])
- **What Was Done**:
  - Created `Assets/Project/Scripts/AI/README.md` fulfilling Section 16 of `architecture-guide.md`, including:
    - Modular architecture overview and decoupling philosophy.
    - Mermaid relationship diagram illustrating `AIBrainBase` coordinating modular components and inventory/events.
    - Component breakdown table mapping base contracts to concrete implementations and roles.
    - Step-by-step User Manuals (Guides 1–5): creating new movement strategies (e.g. rooted/turret), new attack mechanics (e.g. ranged spit), custom brains/states, new `AIData` configuration ScriptableObjects, and full prefab assembly.
  - Verified all 5 plant monster prefabs (`Cabbage Monster`, `Tier 1`, `Tier 2`, `Tier 3`, `Test Plant Monster`):
    - Confirmed each prefab contains all 6 component script GUIDs without broken or missing MonoBehaviour references.
    - Confirmed property mappings (`data`, `movement`, `detection`, `attack`, `animator`, `health`) serialize polymorphically into base class fields.

### TASK-007 / TASK-008 / TASK-009: Plant AI Framework Refactor (Tasks 02, 03, 04) — 2026-10-28 14:25
- **Target Files**:
  - [`Assets/Project/Scripts/AI/AIBrainBase.cs`](../../Assets/Project/Scripts/AI/AIBrainBase.cs) ([NEW])
  - [`Assets/Project/Scripts/AI/AI Movement.cs`](../../Assets/Project/Scripts/AI/AI%20Movement.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/AI/AI Attack.cs`](../../Assets/Project/Scripts/AI/AI%20Attack.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/AI/AI Detection.cs`](../../Assets/Project/Scripts/AI/AI%20Detection.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/AI/AI Animation Controller.cs`](../../Assets/Project/Scripts/AI/AI%20Animation%20Controller.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/AI/AI Health.cs`](../../Assets/Project/Scripts/AI/AI%20Health.cs) ([MODIFIED])
  - [`Assets/Project/Scripts/AI/AI Brain.cs`](../../Assets/Project/Scripts/AI/AI%20Brain.cs) ([MODIFIED])
- **What Was Done**:
  - **Task 02 (`AIBrainBase.cs`)**:
    - Implemented abstract `AIBrainBase` coordinator inheriting `MonoBehaviour` and `IPoolable`.
    - Declared enums `PlantAIState` and `PlantChaseState`.
    - Added protected serialized component fields (`data`, `movement`, `detection`, `attack`, `animator`, `health`) matching existing prefab field names with `[Header]` and `[Tooltip]`.
    - Implemented auto-component resolution in `Awake()`.
    - Established leak-free event subscription in `OnEnable()` and clean unsubscription in `OnDisable()`.
    - Added state transitions (`ChangeState`, `ChangeChaseState`) and virtual hooks (`OnEnterState`, `OnExitState`, `EnterChaseState`, `ExitChaseState`).
    - Added virtual handlers (`OnTargetDetected`, `OnTargetLost`, `OnAttackEnd`, `OnAttackAnimationHit`, `OnAwakeAnimationEnd`, `OnTakeDamage`, `OnDie`) and pool lifecycle hooks (`RollDrop`, `OnSpawn`, `OnDespawn`).
  - **Task 03 (Concrete AI Components)**:
    - `AIMovement`: Inherits `AIMovementBase`; added `agent` validation for active/enabled/onNavMesh; implemented `Speed`, `IsReachDestination`, `TargetTransform`, `Initialize(Vector3, AIData)`, `MoveTo`, `ReturnToStart`, `StopMoving`, and `RotateToTarget`.
    - `AIAttack`: Inherits `AIAttackBase`; implemented `CanAttack`, `IsAttacking`, `OnAttackEndEvent`, `Initialize(AIData)`, `StartAttack`, `ExecuteAttack`, `StopAttack`, and coroutine cleanup on `OnDisable`.
    - `AIDetection`: Inherits `AIDetectionBase`; implemented `IsPlayerInSight`, `TargetTransform`, `OnTargetDetectedEvent`, `OnTargetLostEvent`, `Initialize(float)`, and `EnableDetect(bool)`.
    - `AIAnimationController`: Inherits `AIAnimationControllerBase`; implemented `PlayAwaken`, `PlayAttack`, `PlayHit`, `PlayDie`, `SetMoving`, and `Reset`.
    - `AIHealth`: Inherits `AIHealthBase`; implemented `IsInvincible`, `CurrentHp`, `MaxHp`, `OnDieEvent`, `OnTakeDamageEvent`, `Initialize(int)`, and `TakeDamage(int)`.
    - Preserved all existing `.meta` files, GUIDs, and backwards-compatible member aliases (`targetTranform`, `ExcuteAttack`, `IsInvicible`, `Hp`).
  - **Task 04 (`AIBrain.cs`)**:
    - Refactored `AIBrain` to inherit from `AIBrainBase`, removing redundant enum declarations and delegating lifecycle and event coordination to base class.
    - Implemented state and chase sub-state coroutine orchestration (`ChaseTargetCoroutine`, `AnimationSpeedCoroutine`, `StunCoroutine`, `DieCoroutine`).
    - Added safe coroutine cleanup in `OnDisable()` and `StopAllCoroutinesSafely()`.

### TASK-006: Plant AI Abstract Base Classes — 2026-10-28 14:10
- **Target Files**:
  - [`Assets/Project/Scripts/AI/AIMovementBase.cs`](../../Assets/Project/Scripts/AI/AIMovementBase.cs) ([NEW])
  - [`Assets/Project/Scripts/AI/AIAttackBase.cs`](../../Assets/Project/Scripts/AI/AIAttackBase.cs) ([NEW])
  - [`Assets/Project/Scripts/AI/AIDetectionBase.cs`](../../Assets/Project/Scripts/AI/AIDetectionBase.cs) ([NEW])
  - [`Assets/Project/Scripts/AI/AIAnimationControllerBase.cs`](../../Assets/Project/Scripts/AI/AIAnimationControllerBase.cs) ([NEW])
  - [`Assets/Project/Scripts/AI/AIHealthBase.cs`](../../Assets/Project/Scripts/AI/AIHealthBase.cs) ([NEW])
- **What Was Done**:
  - Created `AIMovementBase` abstract base class defining locomotion parameters, destination querying, targeting binding, and movement control methods (`MoveTo`, `ReturnToStart`, `StopMoving`, `RotateToTarget`).
  - Created `AIAttackBase` abstract base class defining attack availability, progression state, cooldown handling, and `OnAttackEndEvent`.
  - Created `AIDetectionBase` abstract base class defining perception range, target sensing state, and perception events (`OnTargetDetectedEvent`, `OnTargetLostEvent`).
  - Created `AIAnimationControllerBase` abstract base class defining animation trigger methods (`PlayAwaken`, `PlayAttack`, `PlayHit`, `PlayDie`, `SetMoving`, `Reset`).
  - Created `AIHealthBase` abstract base class implementing `IDamageable`, exposing invincibility state, HP values, damage handling, and health events (`OnDieEvent`, `OnTakeDamageEvent`).
  - Adhered strictly to code styling guidelines with Allman braces, XML documentation on all public members, explicit `#region` boundaries, modal booleans, and zero memory leaks.

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
