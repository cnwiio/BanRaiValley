# Task 02: Continuous Harvesting in PlayerInteractor

## 1. Task Goal
Modify `PlayerInteractor.cs` so that holding the interact key (`E`) continuously triggers interaction on focused `IInteractable` targets (such as mature or withered crops) at a rapid interval (~0.15s), allowing the player to harvest entire rows simply by holding E and sweeping the camera.

## 2. Task Information
- **System**: Farming & Player Interaction
- **Parent Plan**: [continuous-planting-harvesting-plan.md](../../plan/continuous-planting-harvesting-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Player/Player Interactor.cs`
- **Dependencies / Prerequisites**:
  - Task 01 must be completed first (`OnInteractActionStartedEvent`, `OnInteractActionCanceledEvent`).
- **Applicable Rules**:
  - `architecture-guide.md`: Event subscriptions in `OnEnable`/`OnDisable`, interface-driven design (`IInteractable`).
  - `naming-variable-rule.md`: Booleans with modal prefixes (`_isInteractHeld`), `_camelCase` for private fields.
  - `function-rule.md`: Single responsibility, CQS separation.

## 3. What To Do (Step-by-Step Instructions)
1. **Field Definitions**:
   - Add `private bool _isInteractHeld;`
   - Use existing `private const float INTERACT_INTERVAL = 0.15f;` and `private float _interactTimer;`.
2. **Lifecycle & Events**:
   - In `OnEnable()`:
     - Subscribe to `OnInteractActionStartedEvent` and `OnInteractActionCanceledEvent`.
   - In `OnDisable()`:
     - Unsubscribe from `OnInteractActionStartedEvent` and `OnInteractActionCanceledEvent`.
     - Reset `_isInteractHeld = false;` and `_interactTimer = 0f;`.
3. **Event Handlers**:
   - `OnInteractActionStarted`: Set `_isInteractHeld = true;`, set `_interactTimer = 0f;`, and trigger `_target?.Interact();`.
   - `OnInteractActionCanceled`: Set `_isInteractHeld = false;`.
4. **Update Loop Interaction**:
   - In `Update()`:
     - Maintain existing raycast and `_target` resolution.
     - When `_target != null` and `_isInteractHeld`:
       - Increment `_interactTimer += Time.deltaTime;`
       - When `_interactTimer >= INTERACT_INTERVAL`:
         - Call `_target.Interact();`
         - Reset `_interactTimer = 0f;`

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] Pressing `E` once still performs a single interaction.
- [ ] Holding `E` and looking at crops continuously harvests ready plants.
- [ ] Releasing `E` immediately stops continuous harvesting.
- [ ] Events properly unsubscribed in `OnDisable`.
