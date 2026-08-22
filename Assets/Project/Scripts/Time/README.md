# Time System — Day/Night Cycle & Calendar

## Overview

The **Time System** is the central calendar and clock simulation for BanRaiValley.
It converts real-world delta time into discrete in-game minutes and drives all
time-sensitive game systems (lighting, UI, farming, AI schedules) exclusively
through **EventBus events** — downstream consumers never poll `CurrentDateTime` in `Update`.

### Architecture Diagram

```
[TimeManager] ──── AdvanceMinute() ────► EventBus<OnTimeTickEvent>
     │                                         │
     │ hour rollover                    ┌──────▼──────────────────────────┐
     ├──────────────────────────────────► EventBus<OnHourChangedEvent>    │
     │                                  └─────────────────────────────────┘
     │ day end (sleep / passout)        ┌──────────────────────────────────┐
     ├──────────────────────────────────► EventBus<OnDayEndedEvent>        │
     │                                  └─────────────────────────────────┘
     │ new morning                      ┌──────────────────────────────────┐
     ├──────────────────────────────────► EventBus<OnNewDayStartedEvent>   │
     │                                  └─────────────────────────────────┘
     │ season rollover                  ┌──────────────────────────────────┐
     ├──────────────────────────────────► EventBus<OnSeasonChangedEvent>   │
     │                                  └─────────────────────────────────┘
     │ forced passout                   ┌──────────────────────────────────┐
     ├──────────────────────────────────► EventBus<OnPlayerPassedOutEvent> │
     │                                  └─────────────────────────────────┘
     │ pause / resume                   ┌──────────────────────────────────────────┐
     └──────────────────────────────────► EventBus<OnTimePausedStateChangedEvent>  │
                                        └──────────────────────────────────────────┘

Listeners (no polling):
  DayNightLightingController ──── OnTimeTickEvent
  TimeCalendarHUD            ──── OnTimeTickEvent, OnNewDayStartedEvent,
                                  OnSeasonChangedEvent, OnTimePausedStateChangedEvent
  PlayerSleepHandler         ──── OnPlayerPassedOutEvent, OnNewDayStartedEvent
```

---

## Folder Structure

| File | Purpose |
|---|---|
| `TimeManager.cs` | Core simulation engine (MonoBehaviour) |
| `TimeDebugController.cs` | Editor / Dev-build keyboard shortcuts |
| `DayNightLightingController.cs` | Drives sun light & ambient sky colours |
| `BedInteractable.cs` | Triggers voluntary sleep on interact |
| `PlayerSleepHandler.cs` | Handles player wake-up, stamina recovery |
| `IInteractable.cs` | Interface implemented by all interactable objects |
| `Data/GameDateTime.cs` | `Season` enum + `GameDateTime` struct |
| `Data/TimeConfiguration.cs` | ScriptableObject — all designer-tunable parameters |
| `UI/TimeCalendarHUD.cs` | HUD view for time, date, season, and pause state |

---

## EventBus Events Reference

| Event | When Raised | Key Payload Fields |
|---|---|---|
| `OnTimeTickEvent` | Every minute tick | `CurrentDateTime`, `NormalizedDayTime` |
| `OnHourChangedEvent` | Each in-game hour rollover | `PreviousHour`, `NewHour`, `CurrentDateTime` |
| `OnDayEndedEvent` | End of day (sleep or passout) | `EndedDateTime`, `IsPassout` |
| `OnNewDayStartedEvent` | Start of a new in-game morning | `NewDateTime`, `WasPassout` |
| `OnSeasonChangedEvent` | Season rollover | `PreviousSeason`, `NewSeason`, `Year` |
| `OnPlayerPassedOutEvent` | Player forced to sleep at passout hour | `PassoutTime`, `StaminaPenaltyPercent` |
| `OnTimePausedStateChangedEvent` | Pause / resume toggled | `IsPaused` |

---

## User Manual / Setup Guide

### Step 1 — Create the `TimeConfiguration` ScriptableObject

1. In the **Project** window: **Right-click → Create → BanRaiValley → Time → Time Configuration**.
2. Name it `TimeConfiguration` and place it in `Assets/Project/Data/Time/`.
3. Configure the fields in the Inspector:

| Field | Recommended Default | Notes |
|---|---|---|
| Real Seconds Per Game Minute | `0.75` | Lower = faster in-game time |
| Minute Tick Interval | `1` | Minutes advanced per tick |
| Start Hour | `6` | Hour when mornings begin (0–23) |
| Passout Hour | `2` | Player forced to sleep at this hour |
| Days Per Season | `30` | Days in each of the 4 seasons |
| Sun Color Gradient | (set key colours) | 0 = midnight, 0.25 = dawn, 0.5 = noon, 0.75 = dusk |
| Sun Intensity Curve | (0→0→1→1→0) | Drives directional light intensity |
| Ambient Sky/Equator/Ground Gradients | (set key colours) | Trilinear ambient sky colours |
| Sun Rotation Euler Offset | `(50, -30, 0)` | Fine-tunes the sun's orbit angle |
| Season Icons | (assign sprites) | Spring / Summer / Fall / Winter UI icons |

---

### Step 2 — Set Up `TimeManager`

1. Create a persistent GameObject in the scene (e.g., `[Managers]`).
2. Add `TimeManager` as a component.
3. Assign the `TimeConfiguration` asset to the **Configuration** field.
4. Set **Auto Start On Awake** to `true` for automatic simulation.

> **Note**: `TimeManager.Update` is the only Update loop in the system. All consumers must subscribe to events — never read `CurrentDateTime` in their own `Update`.

---

### Step 3 — Set Up `DayNightLightingController`

1. Create (or reuse) a **Directional Light** in the scene to act as the sun.
2. On the same or a separate manager GameObject, add `DayNightLightingController`.
3. Assign:
   - **Sun Light** → the Directional Light.
   - **Configuration** → the `TimeConfiguration` asset.
4. `DayNightLightingController` subscribes to `OnTimeTickEvent` automatically and drives sun rotation, colour, intensity, and ambient sky with zero Update polling.

---

### Step 4 — Set Up `BedInteractable` and `PlayerSleepHandler`

#### BedInteractable
1. Place a bed GameObject in the scene.
2. Add the `BedInteractable` component.
3. Assign **Time Manager** → the scene `TimeManager`.
4. Call `Interact(gameObject)` from your player interaction system (via `IInteractable`).

#### PlayerSleepHandler
1. Add `PlayerSleepHandler` to the **Player** GameObject.
2. Assign:
   - **Wake Up Spawn Point** → a Transform marking where the player wakes up.
3. The handler automatically repositions the player and applies stamina recovery on `OnNewDayStartedEvent`.

> **Stamina stubs**: `ApplyPassoutStaminaRecovery` and `ApplyFullStaminaRecovery` contain `TODO` markers — connect them to your `StaminaComponent` when implemented.

---

### Step 5 — Set Up `TimeCalendarHUD`

1. Inside your **Canvas**, create a HUD panel for time display.
2. Add `TimeCalendarHUD` to the panel root GameObject.
3. Assign all UI references in the Inspector:

| Field | Purpose | Example Text |
|---|---|---|
| Time Text | Current in-game time | `"06:00 AM"` or `"06:00"` |
| Day Of Week Text | Abbreviated weekday | `"Mon."` |
| Date Text | Day number within season | `"15"` |
| Season Year Text | Season and year | `"Spring, Year 1"` |
| Season Icon Image | Image for current season | *(sprite from TimeConfiguration)* |
| Pause Indicator | GameObject shown when paused | *(any UI element)* |

4. Assign **Configuration** → the `TimeConfiguration` asset.
5. Toggle **Use 24 Hour Format** to switch between `"18:30"` and `"06:30 PM"`.

---

### Step 6 — (Optional) Set Up `TimeDebugController`

> Only active in **Unity Editor** and **Development Build**. Stripped from release builds automatically.

1. Add `TimeDebugController` to the `[Managers]` GameObject (alongside `TimeManager`).
2. Assign **Time Manager** → the scene `TimeManager`.
3. Set **Enable Debug Hotkeys** to `true` during development.

#### Default Hotkeys

| Key | Action |
|---|---|
| **F5** | Skip 1 hour forward (fires all intermediate events) |
| **F6** | Sleep to next morning (full day transition) |
| **F7** | Toggle time pause on / off |
| **F8** | Toggle fast-forward (1× ↔ configured multiplier, default 10×) |

All keys are reassignable in the Inspector.

---

## Subscribing to Events (Consumer Pattern)

```csharp
using BanRaiValley.Time;

public class MySystem : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus<OnTimeTickEvent>.Subscribe(HandleTimeTick);
        EventBus<OnNewDayStartedEvent>.Subscribe(HandleNewDay);
    }

    private void OnDisable()
    {
        EventBus<OnTimeTickEvent>.Unsubscribe(HandleTimeTick);
        EventBus<OnNewDayStartedEvent>.Unsubscribe(HandleNewDay);
    }

    private void HandleTimeTick(OnTimeTickEvent evt)
    {
        // Use evt.CurrentDateTime and evt.NormalizedDayTime
        // DO NOT read TimeManager.CurrentDateTime here
    }

    private void HandleNewDay(OnNewDayStartedEvent evt)
    {
        // Refresh crops, schedules, etc. using evt.NewDateTime
    }
}
```

---

## Pausing Time

```csharp
// From any script with a reference to TimeManager:
_timeManager.PauseTime(true);   // pause
_timeManager.PauseTime(false);  // resume

// TimeManager also auto-pauses when an InventoryToggleEvent is received.
```

---

## Forcing a Day Transition

```csharp
// Voluntary sleep:
_timeManager.SleepToNextMorning();

// Forced passout (automatic at PassoutHour, but can be triggered manually):
_timeManager.ForcePassout();
```
