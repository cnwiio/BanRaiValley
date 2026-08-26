# Task 07: Plant Growth System Documentation & README

## 1. Task Goal
Create comprehensive architectural documentation and a step-by-step user/designer manual in `Assets/Project/Scripts/Farming/Growth/README.md` explaining system overview, lifecycle flow, ScriptableObject configuration guide, EventBus signatures, and debugging procedures.

## 2. Task Information
- **System**: Plant Growth System
- **Parent Plan**: [.agent/ai-docs/plan/plant-growth-system-plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/plant-growth-system-plan.md)
- **Target File(s)**:
  - `Assets/Project/Scripts/Farming/Growth/README.md`
- **Dependencies / Prerequisites**:
  - Tasks 01–06 completed.
- **Applicable Rules**:
  - [.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md) (Rule 16: README file requirement)
  - [.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)

## 3. What To Do (Step-by-Step Instructions)

1. **Create `Assets/Project/Scripts/Farming/Growth/README.md`**:
   - Must contain:
     - **Overview**: System purpose, architectural diagrams, component breakdown (`CropDataSO`, `CropInstance`, `CropGrowthManager`, `FarmingGrid`).
     - **Growth Lifecycle Flow**: From planting -> daily 6:00 AM hydration check -> stage advancement -> seasonal withering -> harvest awakening.
     - **User Manual & Designer Guide**:
       - Step-by-step instructions on creating a new `CropDataSO` asset in Unity Inspector.
       - Setting up stage visual prefabs, stage durations, compatible seasons, and monster awakening prefabs.
       - Configuring multi-harvest/regrowable crops (e.g. strawberries/berries).
       - Setting up `CropInstance` prefab and registering in `CropGrowthManager`.
     - **EventBus Reference**: Complete table of all static events, payloads, and lifecycle guidelines.
     - **Troubleshooting & FAQs**: Common issues, soil hydration debugging, out-of-season behavior.

## 4. Verification & Testing Checklist
- [ ] README file created with rich Markdown formatting, tables, and Mermaid flowcharts.
- [ ] All setup steps are clearly documented for game designers and programmers.
- [ ] Conforms to Rule 16 of `architecture-guide.md`.
