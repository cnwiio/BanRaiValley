# Task 01: EventBus Signals & PlayerInputReader Hold Tracking

## 1. Task Goal
Add started and canceled events for Primary Action and Interact Action to `EventBus.cs`, and update `PlayerInputReader.cs` to track press states and raise corresponding events so gameplay systems can react to continuous hold input cleanly.

## 2. Task Information
- **System**: Farming & Player Interaction
- **Parent Plan**: [continuous-planting-harvesting-plan.md](../../plan/continuous-planting-harvesting-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/EventBus.cs`
  - `Assets/Project/Scripts/Player/Player Input Reader.cs`
- **Dependencies / Prerequisites**:
  - None
- **Applicable Rules**:
  - `architecture-guide.md`: Event-driven architecture, static `EventBus<T>`, no polling in Update.
  - `naming-variable-rule.md`: Booleans with modal verbs (`is`, `has`, `can`), no Hungarian notation.
  - `function-rule.md`: Single responsibility, explicit parameters.

## 3. What To Do (Step-by-Step Instructions)
1. **Extend `EventBus.cs`**:
   - Add new structs implementing `IEvent`:
     ```csharp
     public struct OnPrimaryActionStartedEvent : IEvent { }
     public struct OnPrimaryActionCanceledEvent : IEvent { }
     public struct OnInteractActionStartedEvent : IEvent { }
     public struct OnInteractActionCanceledEvent : IEvent { }
     ```
2. **Update `PlayerInputReader.cs`**:
   - Add public properties with private setters:
     ```csharp
     public bool IsPrimaryActionPressed { get; private set; }
     public bool IsInteractPressed { get; private set; }
     ```
   - In `OnEnable()`:
     - Register `PrimaryAction.action.started += OnPrimaryActionStarted;`
     - Register `PrimaryAction.action.canceled += OnPrimaryActionCanceled;`
     - Register `InteractAction_PlayerMap.action.started += OnInteractActionStarted;`
     - Register `InteractAction_PlayerMap.action.canceled += OnInteractActionCanceled;`
   - In `OnDisable()`:
     - Unregister all added started/canceled callbacks.
     - Reset `IsPrimaryActionPressed = false;` and `IsInteractPressed = false;`.
   - Implement handler methods raising `EventBus<T>.Raise(...)`.

## 4. Verification & Testing Checklist
- [ ] Script compiles with zero warnings/errors in Unity 6.3.
- [ ] All private fields follow `_camelCase` naming.
- [ ] Events properly unsubscribed in `OnDisable`.
- [ ] Pressing and releasing LMB and E raises started and canceled events respectively.
