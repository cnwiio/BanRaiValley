# Task 07: Plant AI Module Documentation & Readme

## 1. Task Goal
Create the required `README.md` file in `Assets/Project/Scripts/AI/PlantAI/README.md` complying strictly with Rule 16 of the BanRaiValley Architecture Guide, documenting the system overview, architecture structure, inspector setup, and user manual for designers/developers.

---

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [.agent/ai-docs/plan/plant-ai-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-ai-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/PlantAI/README.md`
- **Dependencies / Prerequisites**:
  - Task 01–06 completion
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 16: Adding Readme file — each folder/category must contain overview and user manual)

---

## 3. What To Do (Step-by-Step Instructions)

### Step 1: Create `Assets/Project/Scripts/AI/PlantAI/README.md`
Create the file with the following standard sections:
1. **Overview**:
   - What the Plant AI system does in the context of "The Living Harvest" core loop.
   - Component responsibilities (Perception, Decision/Brain, Locomotion, Combat, Health).
2. **Architecture & Design Principles**:
   - Explanation of zero-Update-polling event & timer architecture.
   - Diagram / Flow of state transitions (`Dormant` -> `Awakening` -> `Idle` -> `Chase` -> `Attack` -> `Dead`).
   - Interface contracts (`IDamageable`, `IAIAgent`).
3. **Inspector Setup & User Manual**:
   - Step-by-step instructions for creating new plant monster prefabs in Unity.
   - How to create and configure `PlantAIConfigSO` and `PlantLootTableSO`.
   - Attaching components (`PlantBrain`, `PlantHealth`, `PlantPerception`, `PlantMovement`, `PlantCombat`, `NavMeshAgent`, `Collider`).
   - How to configure trigger zones and layers for player detection.
4. **Event Reference**:
   - Listing of all events dispatched through `EventBus<T>`.

---

## 4. Verification & Testing Checklist
- [ ] `README.md` is present directly in `Assets/Project/Scripts/AI/PlantAI/`.
- [ ] Contains both high-level system overview and concrete step-by-step user manual as required by Rule 16.
