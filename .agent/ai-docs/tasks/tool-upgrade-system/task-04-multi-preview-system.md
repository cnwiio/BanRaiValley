# Task 04: Multi-Cell Hologram Preview System

## 1. Task Goal
Add multi-tile preview events to [EventBus.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/EventBus.cs) and update [PlacementPreviewer.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Placement%20Previewer.cs) to pool multiple hologram instances using LeanPool, displaying green/red validity for each individual cell in the multi-tile area.

## 2. Task Information
- **System**: Tool Upgrade System
- **Parent Plan**: [.agent/ai-docs/plan/tool-upgrade-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/tool-upgrade-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/EventBus.cs` [MODIFY]
  - `Assets/Project/Scripts/Farming/Placement Previewer.cs` [MODIFY]
- **Dependencies / Prerequisites**:
  - Existing [EventBus.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/EventBus.cs) and [PlacementPreviewer.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Placement%20Previewer.cs).
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Object pooling via LeanPool, zero leaks, event unsubscription)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Handler pattern, `_camelCase` private fields)

## 3. What To Do (Step-by-Step Instructions)

1. **Update `EventBus.cs`**:
   - In `Assets/Project/Scripts/EventBus.cs`, add the following structs near `PreviewingEvent`:
     ```csharp
     public struct TilePreviewData
     {
         public Vector3 Position;
         public bool IsValid;
     }

     public struct MultiPreviewingEvent : IEvent
     {
         public TilePreviewData[] PreviewTiles;
         public int TileCount;
         public float YRotation;
     }
     ```

2. **Update `PlacementPreviewer.cs`**:
   - Replace single-instance `GameObject _hologramPrefabs` with a pooled list:
     ```csharp
     private readonly List<GameObject> _activeHolograms = new List<GameObject>(25);
     private readonly List<MeshRenderer> _activeRenderers = new List<MeshRenderer>(25);
     private GameObject _currentPrefabSource;
     ```
   - In `OnEnable`, subscribe to `EventBus<MultiPreviewingEvent>.Subscribe(OnMultiPreviewing);`.
   - In `OnDisable`, unsubscribe from `EventBus<MultiPreviewingEvent>.Unsubscribe(OnMultiPreviewing);` and invoke `DespawnAllHolograms()`.
   - In `OnStartPreview(StartPreviewEvent evt)`:
     - Store `_currentPrefabSource = evt.prefabs;`
     - Register strategy from `evt.previewState`.
   - Implement `OnMultiPreviewing(MultiPreviewingEvent evt)`:
     - Synchronize `_activeHolograms.Count` to `evt.TileCount`:
       - If `_activeHolograms.Count < evt.TileCount`: spawn needed instances via `LeanPool.Spawn(_currentPrefabSource)`. Cache their `MeshRenderer` in `_activeRenderers`.
       - If `_activeHolograms.Count > evt.TileCount`: despawn excess instances via `LeanPool.Despawn` from the tail and remove from lists.
     - For each index `i` from `0` to `evt.TileCount - 1`:
       - `_activeHolograms[i].transform.position = evt.PreviewTiles[i].Position;`
       - `_activeHolograms[i].transform.rotation = Quaternion.Euler(0f, evt.YRotation, 0f);`
       - `_currentStrategy.Apply(_activeHolograms[i], _activeRenderers[i], validMaterial, inValidMaterial, evt.PreviewTiles[i].IsValid);`
       - `_activeHolograms[i].SetActive(true);`
   - Keep `OnPreviewing(PreviewingEvent evt)` as a backwards-compatible fallback that invokes `OnMultiPreviewing` with a 1-tile temporary representation.
   - In `OnEndPreview(EndPreviewEvent evt)`, invoke `DespawnAllHolograms()`.
   - Helper `DespawnAllHolograms()`:
     ```csharp
     private void DespawnAllHolograms()
     {
         for (int i = 0; i < _activeHolograms.Count; i++)
         {
             if (_activeHolograms[i] != null)
             {
                 LeanPool.Despawn(_activeHolograms[i]);
             }
         }
         _activeHolograms.Clear();
         _activeRenderers.Clear();
         _currentPrefabSource = null;
         _currentStrategy = null;
     }
     ```

## 4. Verification & Testing Checklist
- [ ] Script compiles cleanly with zero warnings.
- [ ] `EventBus<MultiPreviewingEvent>` correctly subscribed in `OnEnable` and unsubscribed in `OnDisable`.
- [ ] All spawned hologram GameObjects are safely returned to `LeanPool` when preview ends or component is disabled.
- [ ] Existing single-tile tools (e.g. SeedBag) continue to preview correctly without regression.
