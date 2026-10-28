# Manual: Shadow and Lighting Configuration Guide

This manual documents the shadow, lighting, and camera configuration for **BanRaiValley** in Unity. Follow these instructions if you ever need to inspect, adjust, or replicate the fix in the Unity Editor.

---

## 1. Terrain Shadow Casting (Preventing Camera Shimmer / Jumping Shadows)

### What it does
By default, Unity Terrains are the ground level intended to receive shadows from characters, trees, and buildings. If a massive terrain (e.g. 1000m x 1000m) has `ShadowCastingMode` set to `TwoSided`, dynamic heightmap LOD patches will recalculate bounding boxes and cull as the camera rotates, causing giant shadow patches to flicker and jump out of place.

### How to configure in the Unity Editor
1. In the **Hierarchy** window, select your active terrain:
   - GameObject: `Terrain (1)` (or `Terrain`).
2. In the **Inspector**, locate the **Terrain** component.
3. Click the **Terrain Settings** icon (the gear / wrench icon at the far right of the Terrain toolbar).
4. Scroll down to the **Lighting** section.
5. Find **Cast Shadows** and ensure it is set to **Off**:
   - `Cast Shadows` → **Off**
6. Ensure **Receive Global Illumination** (or dynamic shadows) is checked so that trees, props, buildings, and the player continue to cast crisp shadows onto the ground.

---

## 2. Day / Night Sun Traversal (Smooth vs Discrete)

### Architecture
- **TimeManager (`Assets/Project/Scripts/Time/Time Manager.cs`)**:
  - Manages calendar dates, seasons, hours, and discrete minute ticks.
  - Keeps the HUD clock discrete (advancing every 10 minutes, e.g. 06:00, 06:10).
  - Exposes `ContinuousNormalizedDayTime` which incorporates fractional real-time progress between minute ticks.
- **DayNightLightingController (`Assets/Project/Scripts/Time/DayNightLightingController.cs`)**:
  - Updates the celestial Directional Light (`_sunDirectionalLight`) per frame using `UpdateContinuousLighting(float continuousNormalizedTime)`.
  - Snaps immediately upon day reset or waking up via `SetSunRotation()`.

### How to configure in the Unity Editor
1. In the **Hierarchy**, select `Directional Light` (contains `DayNightLightingController`).
2. Verify the references:
   - **Sun Directional Light**: Connected to the `Light` component.
   - **Configuration**: `TimeConfiguration.asset` (`Assets/Project/Data/TimeConfiguration.asset`).
   - **Sun Rotation Offset Degrees**: `-90` (places sunrise at 06:00 AM on the horizon).
   - **Sun Yaw Degrees**: `-30` (controls the angle / compass heading of the sun orbit).
3. In `TimeManager`:
   - **Day Night Lighting Controller**: Assigned to `Directional Light`.
   - **Time UI**: Assigned to HUD `TimeUI` component.

---

## 3. Camera Anti-Aliasing (Optional Extra Smoothing for Foliage Edges)

If you notice subtle pixel crawling along sharp alpha-clipped grass blades or tree leaves when panning:
1. Select the `Main Camera` in the **Hierarchy**.
2. In the **Inspector**, locate the **Camera** component under the **Rendering** or **Output** section.
3. Set **Anti-aliasing** to **Subpixel Morphological Anti-aliasing (SMAA)** or **Temporal Anti-aliasing (TAA)**.
