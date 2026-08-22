# Task 04: Bed Interactable & Player Sleep/Passout Handler

## 1. Task Goal
Implement `BedInteractable` (implementing `IInteractable` so the player can interact with their bed in first-person to sleep until morning) and `PlayerSleepHandler` to manage stamina/HP recovery, passout penalties (50% max stamina penalty), and player repositioning on new day transitions.

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/BedInteractable.cs`
  - `Assets/Project/Scripts/Time/PlayerSleepHandler.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`OnPlayerPassedOutEvent`, `OnNewDayStartedEvent`, `OnDayEndedEvent`)
  - Task 02 (`TimeManager`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Interface `IInteractable`, no god classes)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `BedInteractable.cs`
- In `Assets/Project/Scripts/Time/BedInteractable.cs`:
  - `namespace BanRaiValley.Time`.
  - Class: `public class BedInteractable : MonoBehaviour, IInteractable`.
  - Serialized fields:
    - `[SerializeField] private string _interactionLabel = "Sleep (Rest until 6:00 AM)";`
    - `[SerializeField] private TimeManager _timeManager;`
  - Public interface implementation:
    - `public string InteractionLabel => _interactionLabel;`
    - `public bool CanInteract(GameObject interactor) => true;`
    - `public void Interact(GameObject interactor)`:
      - If `_timeManager != null`:
        - `_timeManager.SleepToNextMorning();`
      - Else:
        - Log warning `[BedInteractable] TimeManager reference is missing.`

### Step 2: Implement `PlayerSleepHandler.cs`
- In `Assets/Project/Scripts/Time/PlayerSleepHandler.cs`:
  - `namespace BanRaiValley.Time`.
  - Class: `public class PlayerSleepHandler : MonoBehaviour`.
  - Serialized fields:
    - `[SerializeField] private Transform _wakeUpSpawnPoint;`
    - `[SerializeField] private Transform _playerTransform;`
  - Private state:
    - `private bool _hasPendingPassoutPenalty;`
  - Lifecycle:
    - `OnEnable()`:
      - `EventBus<OnPlayerPassedOutEvent>.Subscribe(HandlePlayerPassedOut);`
      - `EventBus<OnNewDayStartedEvent>.Subscribe(HandleNewDayStarted);`
    - `OnDisable()`:
      - `EventBus<OnPlayerPassedOutEvent>.Unsubscribe(HandlePlayerPassedOut);`
      - `EventBus<OnNewDayStartedEvent>.Unsubscribe(HandleNewDayStarted);`
  - Event Handlers:
    - `private void HandlePlayerPassedOut(OnPlayerPassedOutEvent evt)`:
      - Set `_hasPendingPassoutPenalty = true;`
      - Log: `"[PlayerSleepHandler] Player passed out at 2:00 AM. 50% stamina penalty applied."`
    - `private void HandleNewDayStarted(OnNewDayStartedEvent evt)`:
      - If `_wakeUpSpawnPoint != null && _playerTransform != null`:
        - Reposition player: `_playerTransform.position = _wakeUpSpawnPoint.position;`
        - `_playerTransform.rotation = _wakeUpSpawnPoint.rotation;`
      - If (evt.WasPassout || _hasPendingPassoutPenalty):
        - Handle partial stamina restoration (e.g. 50% penalty).
        - `_hasPendingPassoutPenalty = false;`
      - Else:
        - Handle 100% full stamina/HP restoration.

## 4. Verification & Testing Checklist
- [ ] `BedInteractable` implements `IInteractable` properly.
- [ ] Interacting with the bed invokes `SleepToNextMorning()` on `TimeManager`.
- [ ] `PlayerSleepHandler` repositions player to the bed/wake point upon waking up.
- [ ] Event listeners unsubscribed properly in `OnDisable`.
