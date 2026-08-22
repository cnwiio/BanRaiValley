# Task Overview & Completed Work

This file tracks all completed tasks performed by Coder Agents across the project. Other agents can read this file to understand the current implementation state, modified files, and recent system additions.

---

## Completed Tasks Summary Table

| Task ID / Name | System / Feature | Files Created / Modified | Completed Date |
| :--- | :--- | :--- | :--- |
| Task 01 — Time Data Models & Events | Day/Night Cycle & Calendar | `GameDateTime.cs` [NEW], `TimeConfiguration.cs` [NEW], `EventBus.cs` [MODIFIED] | 2026-08-22 |
| Task 02 — Core TimeManager Service | Day/Night Cycle & Calendar | `TimeManager.cs` [NEW], `Time/README.md` [NEW] | 2026-08-22 |
| Task 03 — DayNightLightingController | Day/Night Cycle & Calendar | `DayNightLightingController.cs` [NEW] | 2026-08-22 |
| Task 04 — Bed Interactable & Sleep Handler | Day/Night Cycle & Calendar | `IInteractable.cs` [NEW], `BedInteractable.cs` [NEW], `PlayerSleepHandler.cs` [NEW] | 2026-08-22 |
| Task 05 — Clock & Calendar HUD | Day/Night Cycle & Calendar | `TimeCalendarHUD.cs` [NEW] | 2026-08-22 |
| Task 06 — Debug Controller & Documentation | Day/Night Cycle & Calendar | `TimeDebugController.cs` [NEW], `TimeManager.cs` [MODIFIED], `Time/README.md` [OVERHAUL] | 2026-08-22 |

---

## Detailed Task Changelog

<!-- New completed task entries are appended below chronologically -->

### Task 01 — Time Data Models & Events — 2026-08-22 21:52
- **Target Files**:
  - [`Assets/Project/Scripts/Time/Data/GameDateTime.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/Data/GameDateTime.cs) [NEW]
  - [`Assets/Project/Scripts/Time/Data/TimeConfiguration.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/Data/TimeConfiguration.cs) [NEW]
  - [`Assets/Project/Scripts/EventBus.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/EventBus.cs) [MODIFIED]
- **What Was Done**:
  - Created `Season` enum (Spring/Summer/Fall/Winter) and `[Serializable] GameDateTime` struct inside `namespace BanRaiValley.Time`.
  - Added computed properties: `TotalDaysPassed`, `NormalizedDayTime`, `IsDayTime`, `IsNightTime`.
  - Added formatting methods: `ToTimeString(bool use24HourFormat)` and `ToDateString()`.
  - Added static factory `GameDateTime.InitialDate` (Year 1, Spring, Day 1, Monday, 06:00).
  - Created `TimeConfiguration` ScriptableObject with `_camelCase` serialized fields and public getters for: simulation timing, day boundaries, sun/ambient lighting gradients & curves, sun rotation offset, and season UI sprites.
  - Added `GetSeasonIcon(Season season)` helper method via switch expression.
  - Added `using BanRaiValley.Time;` to `EventBus.cs`.
  - Appended `#region Time & Calendar Events` to `EventBus.cs` with 7 strong-typed `IEvent` structs: `OnTimeTickEvent`, `OnHourChangedEvent`, `OnDayEndedEvent`, `OnNewDayStartedEvent`, `OnSeasonChangedEvent`, `OnPlayerPassedOutEvent`, `OnTimePausedStateChangedEvent`.

### Task 02 — Core TimeManager Service — 2026-08-22 21:56
- **Target Files**:
  - [`Assets/Project/Scripts/Time/TimeManager.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/TimeManager.cs) [NEW]
  - [`Assets/Project/Scripts/Time/README.md`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/README.md) [NEW]
- **What Was Done**:
  - Implemented `TimeManager : MonoBehaviour` as the central simulation engine for the Day/Night system.
  - Accumulates `Time.deltaTime * _timeScaleMultiplier` each frame; fires `AdvanceMinute()` per tick interval.
  - `AdvanceMinute()` handles minute-to-hour roll-over and raises `OnHourChangedEvent` and `OnTimeTickEvent`.
  - Passout auto-detection: when `Hour == _configuration.PassoutHour && !_hasPassedOutToday`, calls `ForcePassout()`.
  - `SleepToNextMorning()` and `ForcePassout()` both call shared `AdvanceToNextDay(wasPassout)` for DRY calendar logic.
  - `AdvanceToNextDay` handles day-of-season increment, `DayOfWeek` cycling, season roll-over, year increment (Winter→Spring), and raises `OnSeasonChangedEvent`, `OnNewDayStartedEvent`, and an initial `OnTimeTickEvent`.
  - `PauseTime(bool)` guards against no-op calls; raises `OnTimePausedStateChangedEvent`.
  - `SetTimeScaleMultiplier(float)` clamps to ≥ 0 for safe fast-forward support.
  - Auto-pauses on `InventoryToggleEvent` (subscribed in `OnEnable`, unsubscribed in `OnDisable` — zero leak).
  - Created `Time/README.md` with overview, setup steps, and EventBus event reference table.

### Task 03 — DayNightLightingController — 2026-08-22 21:57
- **Target Files**:
  - [`Assets/Project/Scripts/Time/DayNightLightingController.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/DayNightLightingController.cs) [NEW]
- **What Was Done**:
  - Implemented `DayNightLightingController : MonoBehaviour` in `namespace BanRaiValley.Time`.
  - Subscribes to `OnTimeTickEvent` in `OnEnable`, unsubscribes in `OnDisable` (zero leak).
  - `HandleTimeTick` delegates to `EvaluateLighting(normalizedTime)` — no polling in `Update`.
  - `ApplySunRotation`: maps `normalizedTime * 360 + _sunRotationOffsetDegrees` to Directional Light pitch, fixed yaw via `_sunYawDegrees`.
  - `ApplySunLightProperties`: evaluates `SunColorGradient` and `SunIntensityCurve` from `TimeConfiguration`; auto-toggles `LightShadows.Soft / None` based on `SHADOW_INTENSITY_THRESHOLD` constant when `_disableShadowsAtNight` is enabled.
  - `ApplyAmbientLighting`: sets `RenderSettings.ambientMode = Trilight` then drives Sky, Equator, and Ground colours from the three `TimeConfiguration` ambient gradients.
  - All three sub-methods guard against null references before touching Unity APIs.
  - `EvaluateLighting` is `public` to allow direct calls for editor preview or scene initialisation.

### Task 04 — Bed Interactable & Sleep Handler — 2026-08-22 22:00
- **Target Files**:
  - [`Assets/Project/Scripts/Time/IInteractable.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/IInteractable.cs) [NEW]
  - [`Assets/Project/Scripts/Time/BedInteractable.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/BedInteractable.cs) [NEW]
  - [`Assets/Project/Scripts/Time/PlayerSleepHandler.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/PlayerSleepHandler.cs) [NEW]
- **What Was Done**:
  - Created `IInteractable` interface (global, no namespace) with `InteractionLabel`, `CanInteract(GameObject)`, and `Interact(GameObject)` — no existing definition found in codebase.
  - Implemented `BedInteractable : MonoBehaviour, IInteractable` in `namespace BanRaiValley.Time`; `Interact()` calls `_timeManager.SleepToNextMorning()` with null-guard + warning log.
  - Implemented `PlayerSleepHandler : MonoBehaviour` in `namespace BanRaiValley.Time`.
  - Subscribes to `OnPlayerPassedOutEvent` and `OnNewDayStartedEvent` in `OnEnable`, unsubscribes in `OnDisable` (zero leak).
  - `HandlePlayerPassedOut` sets `_hasPendingPassoutPenalty = true` as a bridging flag between the two events.
  - `HandleNewDayStarted` repositions player to `_wakeUpSpawnPoint` (position + rotation), then branches on `evt.WasPassout || _hasPendingPassoutPenalty` to call either `ApplyPassoutStaminaRecovery()` (50%) or `ApplyFullStaminaRecovery()` (100%).
  - Stamina methods contain `TODO` stubs with clear connection points for a future `StaminaComponent`.
  - All null-guard checks emit descriptive `Debug.LogWarning` with component context.

### Task 05 — Clock & Calendar HUD — 2026-08-22 22:03
- **Target Files**:
  - [`Assets/Project/Scripts/Time/UI/TimeCalendarHUD.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/UI/TimeCalendarHUD.cs) [NEW]
- **What Was Done**:
  - Created `TimeCalendarHUD : MonoBehaviour` in `namespace BanRaiValley.Time.UI` — strictly zero `Update()` polling.
  - Subscribes to `OnTimeTickEvent`, `OnNewDayStartedEvent`, `OnSeasonChangedEvent`, and `OnTimePausedStateChangedEvent` in `OnEnable`; unsubscribes in `OnDisable` (zero memory leak).
  - `HandleTimeTick` updates `_timeText` via `GameDateTime.ToTimeString(_use24HourFormat)` (12h AM/PM or 24h configurable via Inspector toggle).
  - `HandleNewDayStarted` calls `RefreshCalendarLabels()` (weekday 3-letter abbreviation + `.`, day-of-season number, season+year string) and `RefreshSeasonIcon()`.
  - `HandleSeasonChanged` updates `_seasonYearText` and refreshes the season icon independently (handles mid-session season rollover without a new-day event).
  - `HandlePauseStateChanged` calls `_pauseIndicator.SetActive(evt.IsPaused)` with null guard.
  - `RefreshCalendarLabels(GameDateTime)` and `RefreshSeasonIcon(Season)` extracted as private helpers for DRY reuse between `HandleNewDayStarted` and `HandleSeasonChanged`.
  - All six UI field assignments are individually null-guarded to prevent NullReferenceExceptions on partial Inspector setups.

### Task 06 — Debug Controller & Documentation — 2026-08-22 22:06
- **Target Files**:
  - [`Assets/Project/Scripts/Time/TimeDebugController.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/TimeDebugController.cs) [NEW]
  - [`Assets/Project/Scripts/Time/TimeManager.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/TimeManager.cs) [MODIFIED]
  - [`Assets/Project/Scripts/Time/README.md`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Time/README.md) [OVERHAUL]
- **What Was Done**:
  - Created `TimeDebugController : MonoBehaviour` in `namespace BanRaiValley.Time`; all input handling wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` — zero runtime overhead in release builds.
  - Serialized hotkeys (`F5`/`F6`/`F7`/`F8`) and `_fastForwardMultiplier` are all Inspector-reassignable.
  - `_enableDebugHotkeys` master toggle allows disabling all shortcuts without removing the component.
  - `SkipOneHour()` → calls `_timeManager.DebugAdvanceMinutes(60)`, firing all intermediate EventBus events.
  - `SkipToMorning()` → calls `_timeManager.SleepToNextMorning()` (full day-end / day-start cycle).
  - `TogglePause()` → `_timeManager.PauseTime(!_timeManager.IsPaused)` with console log.
  - `ToggleFastForward()` → toggles `_isFastForwarding` bool; calls `_timeManager.SetTimeScaleMultiplier(10f or 1f)`.
  - Added `public void DebugAdvanceMinutes(int minutes)` to `TimeManager.cs`, guarded by `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, delegating to the existing private `AdvanceMinute(int)` — no architectural change.
  - Overhauled `Time/README.md` (Rule 16 compliance): full ASCII architecture diagram showing EventBus event flow, complete folder structure table, 7-event EventBus reference table, 6-step setup guide (TimeConfiguration → TimeManager → DayNightLightingController → Bed/Sleep → HUD → DebugController), debug hotkey table, consumer subscription pattern code sample, and API quick-reference snippets.
