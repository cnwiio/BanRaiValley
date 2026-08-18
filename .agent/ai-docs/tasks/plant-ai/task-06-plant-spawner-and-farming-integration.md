# Task 06: Awakened Crop Harvest Spawner & Farming Integration

## 1. Task Goal
Integrate the Plant AI system with the existing farming pipeline ([FarmingGrid](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/Farming%20Grid.cs) and [PlantManager](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Plant%20Manager.cs)) so that harvesting a mature crop initiates the awakening sequence, converts the static plant into an active AI monster, and clears the farming tile upon monster defeat.

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/AwakenedCropHarvestTrigger.cs`
  - `Assets/Project/Scripts/AI/PlantAI/PlantSpawner.cs`
- **Dependencies / Prerequisites**:
  - Task 01 (`EventBus.cs` with `OnPlantAwakenedEvent`, `OnPlantDiedEvent`, `OnClearPlant`)
  - Task 05 (`PlantBrain.cs`)
  - Existing [PlantManager.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Plant%20Manager.cs) & [IFarmingGrid.cs](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/Farming/Gird/IFarmingGrid.cs)
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 2: Global EventBus, Rule 8: Object pooling with LeanPool, Rule 11: Dependency injection)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) (Descriptive event and variable naming)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Implement `AwakenedCropHarvestTrigger.cs`
Create `Assets/Project/Scripts/AI/PlantAI/AwakenedCropHarvestTrigger.cs`:
- Placed on the mature crop prefab or interaction detector.
- **Fields**:
  - `[SerializeField] private GameObject _awakenedMonsterPrefab;`
  - `private Vector3Int _cellPos;`
  - `private bool _isAwakened;`
- **Methods**:
  - `public void Initialize(Vector3Int cellPos)`: Stores the grid cell position.
  - `public void TriggerAwakening()`:
    - Guard `if (_isAwakened) return; _isAwakened = true;`
    - Despawn the static crop visual using `EventBus<OnClearPlant>.Raise(new OnClearPlant { CellPos = _cellPos });`
    - Spawn the monster instance via `LeanPool.Spawn(_awakenedMonsterPrefab, transform.position, transform.rotation)`.
    - Retrieve `PlantBrain` on the spawned monster and call `brain.Awaken()`.
    - Raise `EventBus<OnPlantAwakenedEvent>.Raise(new OnPlantAwakenedEvent { PlantInstance = monster, CellPos = _cellPos, WorldPosition = transform.position });`

### Step 2: Implement `PlantSpawner.cs`
Create `Assets/Project/Scripts/AI/PlantAI/PlantSpawner.cs`:
- Listens to global game events to manage monster pools and clear grid tiles upon monster defeat.
- **Subscriptions** in `OnEnable` / `OnDisable`:
  - Subscribe to `EventBus<OnPlantDiedEvent>`:
    - When `OnPlantDiedEvent` is received:
      - Notify `IFarmingGrid` or raise `OnTileClearEvent` for the monster's `CellPos` so the dirt tile returns to an available farm state.
      - Return monster instance to `LeanPool.Despawn(...)` after death animation.

---

## 4. Verification & Testing Checklist
- [ ] Interacting with a mature crop smoothly transitions it into the awakened monster.
- [ ] Defeating the monster raises `OnPlantDiedEvent`, dispenses loot, and clears the farm grid tile.
- [ ] Object pooling through `LeanPool` is preserved with zero heap allocations during runtime spawning/despawning.
- [ ] All event subscriptions are properly paired in `OnEnable` and `OnDisable`.
