# Task 01: Time Data Models, ScriptableObject Config & EventBus Events

## 1. Task Goal
Define the core time structures (`Season` enum, `GameDateTime` struct), `TimeConfiguration` ScriptableObject, and strong-typed `IEvent` structs in `EventBus.cs` to establish the foundational data contracts for the simulation.

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/Data/GameDateTime.cs`
  - `Assets/Project/Scripts/Time/Data/TimeConfiguration.cs`
  - `Assets/Project/Scripts/EventBus.cs`
- **Dependencies / Prerequisites**:
  - Existing `EventBus.cs`
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Create `GameDateTime.cs`
- In `Assets/Project/Scripts/Time/Data/GameDateTime.cs`:
  - Define `namespace BanRaiValley.Time`.
  - Define `public enum Season { Spring = 0, Summer = 1, Fall = 2, Winter = 3 }`.
  - Define `[System.Serializable] public struct GameDateTime`:
    - Public fields: `int Year`, `Season Season`, `int DayOfSeason` (1–30), `System.DayOfWeek DayOfWeek`, `int Hour` (0–23), `int Minute` (0–59).
    - Property: `public int TotalDaysPassed => (Year - 1) * 120 + ((int)Season * 30) + (DayOfSeason - 1);`
    - Property: `public float NormalizedDayTime => (Hour * 60f + Minute) / 1440f;` (0.0 to 1.0).
    - Property: `public bool IsDayTime => Hour >= 6 && Hour < 18;`
    - Property: `public bool IsNightTime => !IsDayTime;`
    - Methods:
      - `public string ToTimeString(bool use24HourFormat = false)` (Formats e.g. "06:00 AM" or "18:30").
      - `public string ToDateString()` (Formats e.g. "Mon, Spring 1, Year 1").
    - Static Property: `public static GameDateTime InitialDate => new GameDateTime { Year = 1, Season = Season.Spring, DayOfSeason = 1, DayOfWeek = System.DayOfWeek.Monday, Hour = 6, Minute = 0 };`

### Step 2: Create `TimeConfiguration.cs`
- In `Assets/Project/Scripts/Time/Data/TimeConfiguration.cs`:
  - Define `[CreateAssetMenu(fileName = "TimeConfiguration", menuName = "BanRaiValley/Time/Time Configuration")] public class TimeConfiguration : ScriptableObject`.
  - Serialized private fields with `_camelCase` naming and public getters:
    - `[SerializeField] private float _realSecondsPerGameMinute = 0.75f;`
    - `[SerializeField] private int _minuteTickInterval = 1;`
    - `[SerializeField] private int _startHour = 6;`
    - `[SerializeField] private int _passoutHour = 2;`
    - `[SerializeField] private int _daysPerSeason = 30;`
    - `[SerializeField] private Gradient _sunColorGradient;`
    - `[SerializeField] private AnimationCurve _sunIntensityCurve;`
    - `[SerializeField] private Gradient _ambientSkyColorGradient;`
    - `[SerializeField] private Gradient _ambientEquatorColorGradient;`
    - `[SerializeField] private Gradient _ambientGroundColorGradient;`
    - `[SerializeField] private Vector3 _sunRotationEulerOffset = new Vector3(50f, -30f, 0f);`
    - `[SerializeField] private Sprite _springIcon;`
    - `[SerializeField] private Sprite _summerIcon;`
    - `[SerializeField] private Sprite _fallIcon;`
    - `[SerializeField] private Sprite _winterIcon;`
  - Helper method: `public Sprite GetSeasonIcon(Season season)`.

### Step 3: Add Time Events to `EventBus.cs`
- Add a new `#region Time & Calendar Events` in `Assets/Project/Scripts/EventBus.cs`:
  - `public struct OnTimeTickEvent : IEvent { public GameDateTime CurrentDateTime; public float NormalizedDayTime; }`
  - `public struct OnHourChangedEvent : IEvent { public int PreviousHour; public int NewHour; public GameDateTime CurrentDateTime; }`
  - `public struct OnDayEndedEvent : IEvent { public GameDateTime EndedDateTime; public bool IsPassout; }`
  - `public struct OnNewDayStartedEvent : IEvent { public GameDateTime NewDateTime; public bool WasPassout; }`
  - `public struct OnSeasonChangedEvent : IEvent { public Season PreviousSeason; public Season NewSeason; public int Year; }`
  - `public struct OnPlayerPassedOutEvent : IEvent { public GameDateTime PassoutTime; public float StaminaPenaltyPercent; }`
  - `public struct OnTimePausedStateChangedEvent : IEvent { public bool IsPaused; }`

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] All private fields follow `_camelCase` naming.
- [ ] `GameDateTime` correctly calculates `NormalizedDayTime` and string representations.
- [ ] All events implement `IEvent` and can be raised via `EventBus<T>.Raise(evt)`.
