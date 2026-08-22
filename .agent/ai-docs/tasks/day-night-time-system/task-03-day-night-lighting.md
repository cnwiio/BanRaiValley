# Task 03: DayNightLightingController (Sun Rotation & Ambient Gradients)

## 1. Task Goal
Implement `DayNightLightingController` which subscribes to `OnTimeTickEvent`, rotates the Directional Sun Light across the sky (0° to 360° pitch/yaw based on time of day), evaluates light color and intensity curves, and smoothly drives ambient lighting gradients (Sky, Equator, Ground) without frame polling.

## 2. Task Information
- **System**: Day/Night Cycle & Calendar Time System
- **Parent Plan**: [.agent/ai-docs/plan/day-night-time-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/day-night-time-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Time/DayNightLightingController.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`TimeConfiguration`, `OnTimeTickEvent`, `GameDateTime`)
  - Task 02 (`TimeManager`)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Define Fields and Dependencies
- In `Assets/Project/Scripts/Time/DayNightLightingController.cs`:
  - `namespace BanRaiValley.Time`.
  - Class: `public class DayNightLightingController : MonoBehaviour`.
  - Serialized fields:
    - `[SerializeField] private Light _sunDirectionalLight;`
    - `[SerializeField] private TimeConfiguration _configuration;`
    - `[Header("Rotation Settings")]`
    - `[SerializeField] private float _sunRotationOffsetDegrees = -90f; // -90 offset so 6:00 AM is sunrise at horizon`
    - `[SerializeField] private float _sunYawDegrees = -30f;`
    - `[Header("Shadow Control")]`
    - `[SerializeField] private bool _disableShadowsAtNight = true;`

### Step 2: Event Subscriptions in `OnEnable` / `OnDisable`
- `OnEnable()`:
  - `EventBus<OnTimeTickEvent>.Subscribe(HandleTimeTick);`
- `OnDisable()`:
  - `EventBus<OnTimeTickEvent>.Unsubscribe(HandleTimeTick);`

### Step 3: Lighting Evaluation Logic
- `private void HandleTimeTick(OnTimeTickEvent evt)`:
  - `EvaluateLighting(evt.NormalizedDayTime);`
- `public void EvaluateLighting(float normalizedTime)`:
  - If `_configuration == null` return;
  - **Sun Rotation**:
    - Calculate angle: `float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;`
    - Apply rotation: If `_sunDirectionalLight != null`:
      - `_sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);`
  - **Sun Light Properties**:
    - If `_configuration.SunColorGradient != null`:
      - `_sunDirectionalLight.color = _configuration.SunColorGradient.Evaluate(normalizedTime);`
    - If `_configuration.SunIntensityCurve != null`:
      - `float intensity = _configuration.SunIntensityCurve.Evaluate(normalizedTime);`
      - `_sunDirectionalLight.intensity = intensity;`
      - If `_disableShadowsAtNight`:
        - `_sunDirectionalLight.shadows = (intensity > 0.05f) ? LightShadows.Soft : LightShadows.None;`
  - **Ambient Lighting**:
    - If `_configuration.AmbientSkyColorGradient != null`:
      - `RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;`
      - `RenderSettings.ambientSkyColor = _configuration.AmbientSkyColorGradient.Evaluate(normalizedTime);`
    - If `_configuration.AmbientEquatorColorGradient != null`:
      - `RenderSettings.ambientEquatorColor = _configuration.AmbientEquatorColorGradient.Evaluate(normalizedTime);`
    - If `_configuration.AmbientGroundColorGradient != null`:
      - `RenderSettings.ambientGroundColor = _configuration.AmbientGroundColorGradient.Evaluate(normalizedTime);`

## 4. Verification & Testing Checklist
- [ ] Compiles cleanly with no errors.
- [ ] No polling in `Update()`; updates strictly on `OnTimeTickEvent`.
- [ ] Directional light rotates smoothly with the passage of in-game time.
- [ ] Sun color, intensity, and ambient colors match the evaluated gradients.
- [ ] Event listeners unsubscribed properly in `OnDisable`.
