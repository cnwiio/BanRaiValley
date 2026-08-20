# BanRaiValley AI Agent Commands & Workflow Registry

Welcome to the **BanRaiValley** AI Agent System. The following workflow commands are available in chat. Whenever the user invokes any of these slash commands, immediately activate the corresponding workflow and role protocol:

---

## 📋 Available Commands & Roles

### 1. `/plan [system-name]` — Lead Developer Agent (Interactive Grill-Me Mode)
- **Workflow File**: [.agent/workflow/plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/plan.md)
- **Role**: Lead Developer & System Architect
- **Action**:
  1. Reads [.agent/docs/GameOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/docs/GameOverview.md) and [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/).
  2. **Interviews the user** using `ask_question` one question at a time across key decision branches (Core Mechanics, Data Models, EventBus, UI/Input) with recommended options.
  3. Creates the **Big Architecture Plan** at `.agent/ai-docs/plan/[system-name]-plan.md` integrating all confirmed choices.
  4. Decomposes the plan into step-by-step **Task Files** for Coder Agents at `.agent/ai-docs/tasks/[system-name]/task-[01-xx]-[name].md`.

---

### 2. `/coding [task-path | text-instructions]` — Coder Agent
- **Workflow File**: [.agent/workflow/coding.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/coding.md)
- **Role**: Fast, Token-Efficient Coder
- **Action**:
  1. Reads the assigned task file or raw text instructions (skips long-term design specs to save tokens).
  2. Implements C# scripts adhering strictly to [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/) (EventBus, no polling in `Update`, `_camelCase` private fields, modal booleans).
  3. Appends completed work entry into the centralized [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md).

---

### 3. `/review [optional: task-name | all]` — Reviewer Agent
- **Workflow File**: [.agent/workflow/review.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/review.md)
- **Role**: Technical QA & Architecture Auditor
- **Action**:
  1. Reads [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and examines modified source files.
  2. Performs 4-Pillar Audit:
     - **Architecture & Memory**: Event unsubscription in `OnDisable`, no polling in `Update`, inheritance $\le 2$.
     - **Performance & GC**: Zero GC allocs in hot loops, cached references.
     - **Rule Compliance**: Variable naming (`_camelCase`), modal booleans, function hygiene.
     - **Plan Adherence**: Matches task specifications and public contracts.
  3. Updates [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md) with `[PASS]` or `[NEEDS REVISION]` + exact actionable fix snippets.

---

### 4. `/fix [optional: task-name]` — Coder Agent (Fix Mode)
- **Workflow File**: [.agent/workflow/fix.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/fix.md)
- **Role**: Bugfix & Revision Specialist
- **Action**:
  1. Reads `NEEDS REVISION` entries in [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md).
  2. Applies the reviewer's exact code fixes while keeping rules intact.
  3. Appends `[FIX]` entry into [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md).

---

### 5. `/clean-report` — Clean & Archive Agent
- **Workflow File**: [.agent/workflow/clean-report.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/clean-report.md)
- **Role**: Milestone Archival & History Reset
- **Action**:
  1. Reads active [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md).
  2. Compiles and saves a comprehensive milestone archive report at `.agent/ai-docs/reports/report-YYYY-MM-DD-HHmm.md`.
  3. Resets `TaskOverview.md` and `TaskReview.md` to their clean, empty initial blank templates.
