# Task 05: Clock & Calendar HUD View and Controller

## 1. Task Goal
Create the `TimeCalendarHUD` UI component that displays in-game time (12h AM/PM or 24h format), season name + icon, weekday, day number (1–30), and year. The HUD must update exclusively via EventBus subscriptions with zero frame polling in `Update`.

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/UI/TimeCalendarHUD.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`GameDateTime`, `TimeConfiguration`, `OnTimeTickEvent`, `OnNewDayStartedEvent`, `OnSeasonChangedEvent`, `OnTimePausedStateChangedEvent`)
  - Task 02 (`TimeManager`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Zero polling in Update, UI listens to EventBus)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Define UI Bindings and Configuration
- In `Assets/Project/Scripts/Time/UI/TimeCalendarHUD.cs`:
  - `using TMPro;`
  - `using UnityEngine.UI;`
  - `namespace BanRaiValley.Time.UI`.
  - Class: `public class TimeCalendarHUD : MonoBehaviour`.
  - Serialized UI Elements:
    - `[SerializeField] private TextMeshProUGUI _timeText;`
    - `[SerializeField] private TextMeshProUGUI _dayOfWeekText;`
    - `[SerializeField] private TextMeshProUGUI _dateText; // e.g. "Day 15"`
    - `[SerializeField] private TextMeshProUGUI _seasonYearText; // e.g. "Spring, Year 1"`
    - `[SerializeField] private Image _seasonIconImage;`
    - `[SerializeField] private GameObject _pauseIndicator;`
    - `[SerializeField] private TimeConfiguration _configuration;`
    - `[SerializeField] private bool _use24HourFormat = false;`

### Step 2: Event Subscriptions in `OnEnable` / `OnDisable`
- `OnEnable()`:
  - `EventBus<OnTimeTickEvent>.Subscribe(HandleTimeTick);`
  - `EventBus<OnNewDayStartedEvent>.Subscribe(HandleNewDayStarted);`
  - `EventBus<OnSeasonChangedEvent>.Subscribe(HandleSeasonChanged);`
  - `EventBus<OnTimePausedStateChangedEvent>.Subscribe(HandlePauseStateChanged);`
- `OnDisable()`:
  - `EventBus<OnTimeTickEvent>.Unsubscribe(HandleTimeTick);`
  - `EventBus<OnNewDayStartedEvent>.Unsubscribe(HandleNewDayStarted);`
  - `EventBus<OnSeasonChangedEvent>.Unsubscribe(HandleSeasonChanged);`
  - `EventBus<OnTimePausedStateChangedEvent>.Unsubscribe(HandlePauseStateChanged);`

### Step 3: Event Handlers and View Refresh
- `private void HandleTimeTick(OnTimeTickEvent evt)`:
  - Update time string: `if (_timeText != null) _timeText.text = evt.CurrentDateTime.ToTimeString(_use24HourFormat);`
- `private void HandleNewDayStarted(OnNewDayStartedEvent evt)`:
  - Update all calendar texts:
    - `if (_dayOfWeekText != null) _dayOfWeekText.text = evt.NewDateTime.DayOfWeek.ToString().Substring(0, 3) + ".";`
    - `if (_dateText != null) _dateText.text = $"{evt.NewDateTime.DayOfSeason}";`
    - `if (_seasonYearText != null) _seasonYearText.text = $"{evt.NewDateTime.Season}, Year {evt.NewDateTime.Year}";`
    - `if (_seasonIconImage != null && _configuration != null) _seasonIconImage.sprite = _configuration.GetSeasonIcon(evt.NewDateTime.Season);`
- `private void HandleSeasonChanged(OnSeasonChangedEvent evt)`:
  - Refresh season icon and text.
- `private void HandlePauseStateChanged(OnTimePausedStateChangedEvent evt)`:
  - `if (_pauseIndicator != null) _pauseIndicator.SetActive(evt.IsPaused);`

## 4. Verification & Testing Checklist
- [ ] No `Update()` method in `TimeCalendarHUD`.
- [ ] Text strings format cleanly and accurately.
- [ ] Season icon changes correctly when season changes.
- [ ] Pause indicator displays when paused.
- [ ] All EventBus subscriptions cleaned up in `OnDisable`.
