# Task 05: Add Architecture Documentation (`README.md`) & Verify Prefabs

## 1. Task Goal
Fulfill Rule 16 of `architecture-guide.md` by creating a comprehensive `README.md` in `Assets/Project/Scripts/AI/` detailing the framework overview, component responsibilities, and step-by-step user manuals for creating new plant monster types. Then, verify that all existing Cabbage Monster prefabs retain their script bindings and serialize without broken references.

## 2. Task Information
- **System**: Plant AI System
- **Parent Plan**: [plant-ai-framework-plan.md](../../plan/plant-ai-framework-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/AI/README.md`
  - `Assets/Project/Prefabs/Plant Monster/Cabbage Monster.prefab`
  - `Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 1.prefab`
  - `Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 2.prefab`
  - `Assets/Project/Prefabs/Plant Monster/Cabbage Monster Tier 3.prefab`
  - `Assets/Project/Prefabs/Plant Monster/Test Plant Monster.prefab`
- **Dependencies / Prerequisites**:
  - Task 01 to 04 completed.
- **Applicable Rules**:
  - `architecture-guide.md`: Section 16 (Adding Readme file with overview and essential user manual steps).

## 3. What To Do (Step-by-Step Instructions)

### 3.1. Create `Assets/Project/Scripts/AI/README.md`
Create documentation containing:
1. **Overview**:
   - Description of the modular Plant AI framework.
   - Core architecture: `AIBrainBase` coordinating `AIMovementBase`, `AIAttackBase`, `AIDetectionBase`, `AIAnimationControllerBase`, `AIHealthBase`.
2. **Architecture Diagram**:
   - Mermaid diagram showing component relationships.
3. **User Manual (How-To Guides)**:
   - **Guide 1: Creating a Monster with a New Movement Type (e.g., Rooted / Spore Turret)**:
     1. Create a new class inheriting from `AIMovementBase`.
     2. Implement `MoveTo`, `ReturnToStart`, `RotateToTarget`, etc.
     3. On the new plant prefab, attach your new movement script instead of `AIMovement`.
     4. Assign the reference to the `AIBrain` component.
   - **Guide 2: Creating a Monster with a New Attack Type (e.g., Ranged Seed Spit)**:
     1. Create a new class inheriting from `AIAttackBase`.
     2. Implement `StartAttack`, `ExecuteAttack` (e.g. projectile spawn from pool), and `StopAttack`.
     3. Attach to prefab and assign to `AIBrain`.
   - **Guide 3: Creating a Monster with Unique States / Brain**:
     1. Inherit from `AIBrainBase`.
     2. Override `OnEnterState` / `OnExitState` or add new state machine logic.
   - **Guide 4: Creating New Monster Config Data**:
     1. Right-click in Project view $\rightarrow$ `Create` $\rightarrow$ `Scriptable Objects` $\rightarrow$ `AIData`.
     2. Configure stats and assign drop item.

### 3.2. Verify Prefabs & Unity Compilation
- Ensure zero compiler errors across the solution.
- Verify that GUID references in `.meta` files for all AI scripts remain matching.
- Inspect prefab files to verify components (`AIBrain`, `AIMovement`, `AIAttack`, `AIDetection`, `AIAnimationController`, `AIHealth`) resolve without `Missing (MonoBehaviour)`.

## 4. Verification & Testing Checklist
- [ ] `Assets/Project/Scripts/AI/README.md` created with Overview and step-by-step User Manual.
- [ ] All 5 Cabbage Monster prefabs load with valid component links.
- [ ] Unity compiles with 0 errors and 0 warnings.
