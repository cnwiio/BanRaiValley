# Task 02: Core TimeManager Service & Simulation Engine

## 1. Task Goal
Implement the central `TimeManager` component responsible for accumulating real time, stepping through minute and hour intervals, managing day/season/year progression, handling pause states, and initiating day transitions (sleep and 2:00 AM passout).

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/TimeManager.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`GameDateTime`, `TimeConfiguration`, and Time Events in `EventBus.cs`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Define Fields and Properties
- In `Assets/Project/Scripts/Time/TimeManager.cs`:
  - `[SerializeField] private TimeConfiguration _configuration;`
  - `[SerializeField] private bool _autoStartOnAwake = true;`
  - Private state:
    - `private GameDateTime _currentDateTime;`
    - `private bool _isPaused;`
    - `private float _minuteAccumulator;`
    - `private float _timeScaleMultiplier = 1f;`
    - `private bool _hasPassedOutToday;`
  - Public properties:
    - `public GameDateTime CurrentDateTime => _currentDateTime;`
    - `public bool IsPaused => _isPaused;`
    - `public float TimeScaleMultiplier => _timeScaleMultiplier;`

### Step 2: Time Progression Logic in `Update`
- In `Update()`:
  - If `_isPaused` or `_configuration == null`, return early.
  - Accumulate delta time: `_minuteAccumulator += Time.deltaTime * _timeScaleMultiplier;`
  - While `_minuteAccumulator >= _configuration.RealSecondsPerGameMinute`:
    - `_minuteAccumulator -= _configuration.RealSecondsPerGameMinute;`
    - Advance time by 1 minute (or `_configuration.MinuteTickInterval`).
    - Check for hour change, passout condition (e.g. `Hour == _configuration.PassoutHour`), and emit tick events.

### Step 3: Minute & Hour Stepping & Calendar Logic
- `private void AdvanceMinute(int minutes)`:
  - Add minutes to `_currentDateTime.Minute`.
  - Handle hour rollover (`Minute >= 60` -> increment `Hour`, modulo 60).
  - If hour changed:
    - Raise `OnHourChangedEvent`.
    - Check passout: If `_currentDateTime.Hour == _configuration.PassoutHour && !_hasPassedOutToday`:
      - Set `_hasPassedOutToday = true;`
      - Call `ForcePassout()`.
  - Raise `OnTimeTickEvent`.

### Step 4: Calendar Advancement & New Day
- `public void SleepToNextMorning()`:
  - Raise `OnDayEndedEvent { EndedDateTime = _currentDateTime, IsPassout = false }`.
  - Advance day:
    - Reset `_currentDateTime.Hour = _configuration.StartHour;`
    - `_currentDateTime.Minute = 0;`
    - `_currentDateTime.DayOfSeason++;`
    - `_currentDateTime.DayOfWeek = (DayOfWeek)(((int)_currentDateTime.DayOfWeek + 1) % 7);`
    - Check Season rollover: If `_currentDateTime.DayOfSeason > _configuration.DaysPerSeason`:
      - `_currentDateTime.DayOfSeason = 1;`
      - Season prev = `_currentDateTime.Season;`
      - `_currentDateTime.Season = (Season)(((int)_currentDateTime.Season + 1) % 4);`
      - If `_currentDateTime.Season == Season.Spring`: increment `_currentDateTime.Year;`
      - Raise `OnSeasonChangedEvent`.
    - `_hasPassedOutToday = false;`
    - `_minuteAccumulator = 0f;`
    - Raise `OnNewDayStartedEvent { NewDateTime = _currentDateTime, WasPassout = false }`.
    - Raise `OnTimeTickEvent`.

- `public void ForcePassout()`:
  - Raise `OnPlayerPassedOutEvent { PassoutTime = _currentDateTime, StaminaPenaltyPercent = 0.5f }`.
  - Raise `OnDayEndedEvent { EndedDateTime = _currentDateTime, IsPassout = true }`.
  - Advance day identically to sleep, but raise `OnNewDayStartedEvent { NewDateTime = _currentDateTime, WasPassout = true }`.

### Step 5: Pause & Event Listeners
- `public void PauseTime(bool isPaused)`:
  - If `_isPaused == isPaused` return;
  - `_isPaused = isPaused;`
  - Raise `OnTimePausedStateChangedEvent { IsPaused = isPaused }`.
- Subscribe to `InventoryToggleEvent` in `OnEnable` / `OnDisable` to auto-pause when inventory is open if modal is active.

## 4. Verification & Testing Checklist
- [ ] Compiles cleanly in Unity 6.3.
- [ ] Time advances correctly according to `RealSecondsPerGameMinute`.
- [ ] Hours and minutes roll over smoothly without losing time fractions.
- [ ] 30-day season cycle rolls over to the next season and increments year after Winter 30.
- [ ] Event listeners unsubscribed properly in `OnDisable`.
- [ ] Zero polling in other classes; all downstream updates come via `OnTimeTickEvent` / `OnNewDayStartedEvent`.
