# Task 06: Developer Time Debug Controller & Subsystem Documentation

## 1. Task Goal
Implement `TimeDebugController` to provide quick keyboard shortcuts in Editor/Development builds for skipping time, skipping days, pausing/resuming, and toggling time acceleration, and create the comprehensive `Assets/Project/Scripts/Time/README.md` documentation file per Rule 16 of the architecture guidelines.

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/TimeDebugController.cs`
  - `Assets/Project/Scripts/Time/README.md`
- **Dependencies / Prerequisites**:
  - Tasks 01–05
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 16: README file requirement with overview and user manual)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `TimeDebugController.cs`
- In `Assets/Project/Scripts/Time/TimeDebugController.cs`:
  - `namespace BanRaiValley.Time`.
  - Class: `public class TimeDebugController : MonoBehaviour`.
  - Serialized fields:
    - `[SerializeField] private TimeManager _timeManager;`
    - `[SerializeField] private bool _enableDebugHotkeys = true;`
    - `[SerializeField] private KeyCode _skipOneHourKey = KeyCode.F5;`
    - `[SerializeField] private KeyCode _skipToMorningKey = KeyCode.F6;`
    - `[SerializeField] private KeyCode _togglePauseKey = KeyCode.F7;`
    - `[SerializeField] private KeyCode _toggleFastForwardKey = KeyCode.F8;`
    - `[SerializeField] private float _fastForwardMultiplier = 10f;`
  - Input Handling in `Update()`:
    - Wrap in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
    - If `!_enableDebugHotkeys || _timeManager == null` return;
    - If `Input.GetKeyDown(_skipOneHourKey)`: Advance time by 60 minutes.
    - If `Input.GetKeyDown(_skipToMorningKey)`: Call `_timeManager.SleepToNextMorning()`.
    - If `Input.GetKeyDown(_togglePauseKey)`: Toggle pause via `_timeManager.PauseTime(!_timeManager.IsPaused)`.
    - If `Input.GetKeyDown(_toggleFastForwardKey)`: Toggle between 1x and `_fastForwardMultiplier` via `_timeManager.SetTimeScaleMultiplier()`.

### Step 2: Create `Assets/Project/Scripts/Time/README.md`
- Provide full subsystem documentation:
  - **Overview**: Purpose, architecture diagram, and event-driven data flow.
  - **User Manual / Setup Guide**:
    - How to configure `TimeConfiguration` ScriptableObject (gradients, time scales, season lengths).
    - Setting up `TimeManager` and assigning the `TimeConfiguration`.
    - Setting up `DayNightLightingController` with Sun Directional Light.
    - Setting up `BedInteractable` and `PlayerSleepHandler`.
    - Setting up `TimeCalendarHUD` on Canvas.
    - Debug Hotkey references (F5, F6, F7, F8).

## 4. Verification & Testing Checklist
- [ ] Debug controls only active in Editor and Development builds.
- [ ] F5 advances hour, F6 sleeps to next morning, F7 pauses, F8 fast-forwards smoothly.
- [ ] `README.md` clearly explains setup and usage of all time subsystem components.
- [ ] Complete system compiles with zero errors in Unity 6.3.
