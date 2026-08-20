# Task 02: Player Hand Visualizer Bare Hand Support

## 1. Task Goal
Update `PlayerHandVisualizer` to display a Minecraft-style Bare Hand prefab when the active hotbar slot is empty (unarmed), and expose the current visualizer GameObject and Animator for attack animation playback.

## 2. Task Information
- **System**: Player Attack System
- **Parent Plan**: [.agent/ai-docs/plan/player-attack-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/player-attack-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Inventory/Player Hand Visualizer.cs` (MODIFY)
- **Dependencies / Prerequisites**:
  - Task 01
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)
  - [.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)
  - [.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Add Bare Hand Serialization & Properties**:
   - In `PlayerHandVisualizer.cs`:
     - Add `[SerializeField] private GameObject _bareHandPrefab;`
     - Cache the active Animator: `private Animator _currentAnimator;`
     - Expose public getters:
       - `public Animator CurrentAnimator => _currentAnimator;`
       - `public GameObject CurrentItemInstance => _currentItem;`
       - `public bool IsHoldingItem => _currentSlotData != null && !_currentSlotData.IsEmpty && _currentSlotData.count > 0;`

2. **Handle Bare Hand Spawning on Empty Slots**:
   - When `SpawnSlotItem(SlotData slotData)` is called:
     - Clear previous `_currentItem` via `LeanPool.Despawn` (or `Destroy` if not pooled).
     - Reset `spawnTransform.localPosition = _initialSpawnPosition;`
     - If `slotData == null || slotData.IsEmpty || slotData.count == 0`:
       - If `_bareHandPrefab != null`:
         - Spawn `_bareHandPrefab` attached to `spawnTransform`.
         - Assign `_currentItem = spawnedBareHand;`
         - Cache `_currentAnimator = _currentItem.GetComponentInChildren<Animator>();`
     - Else (holding an item):
       - If `slotData.item != null && slotData.item.prefab != null`:
         - `spawnTransform.localPosition += slotData.item.spawnOffset;`
         - Spawn `slotData.item.prefab` via `LeanPool.Spawn(slotData.item.prefab, spawnTransform);`
         - Assign `_currentItem = spawnedItem;`
         - Cache `_currentAnimator = _currentItem.GetComponentInChildren<Animator>();`

3. **Provide Attack Animation Helper**:
   - Add public method `public void TriggerAttackAnimation()`:
     - If `_currentAnimator != null`, call `_currentAnimator.SetTrigger("Attack");` (or play attack state).

## 4. Verification & Testing Checklist
- [ ] Empty hotbar slot displays the bare hand model.
- [ ] Switching between empty slot and held tools/seeds properly despawns and spawns the corresponding models.
- [ ] No null reference exceptions when `_bareHandPrefab` or item prefabs lack an Animator.
- [ ] Proper lifecycle cleanup in `OnDisable`.
