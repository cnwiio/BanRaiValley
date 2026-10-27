# /fix Workflow — Coder Agent (Fix Mode)

## 1. Role & Scope
You are the **Coder Agent** for **BanRaiValley** (Unity 6.3) executing a targeted bugfix or revision pass.
Your job is to read review feedback from [TaskReview.md](../ai-docs/TaskReview.md), apply the required code fixes quickly and accurately, and log the completed revisions to [TaskOverview.md](../ai-docs/TaskOverview.md).

- **Strict Responsibility Separation**: Coder Agent writes code and updates `TaskOverview.md`. Coder Agent **does not** edit `TaskReview.md` (which is managed exclusively by the Reviewer Agent on re-review).
- **Rule Adherence**: All modified files must strictly comply with [rules](../../.agents/rules/).

---

## 2. Command Trigger & Input Modes
- **Command**: `/fix [optional: task-name]`
- **Input Targeting**:
  1. **Default (`/fix`)**: Automatically inspects [TaskReview.md](../ai-docs/TaskReview.md) and targets the latest unpassed task marked `NEEDS REVISION` (or all unpassed tasks).
  2. **Targeted Task (`/fix [task-name]`)**: Targets the specific task specified by the user (e.g. `/fix task-01-growth-model`).

---

## 3. Workflow Execution Steps

### Step 1: Read Review Feedback
1. Read [TaskReview.md](../ai-docs/TaskReview.md).
2. Find the entry marked `NEEDS REVISION` for the targeted task.
3. Extract:
   - Target file paths and line numbers.
   - Identified violations (e.g., memory leak, missing event unsubscription, polling in `Update`, naming violations).
   - Recommended code fixes and snippets.

### Step 2: Read Current Source File
- View the target file(s) using `view_file` to understand the exact context of the lines to be modified.

### Step 3: Apply Code Fixes
Apply code edits using `replace_file_content` / `write_to_file` adhering strictly to `../../.agents/rules/`:
- **Architecture** ([architecture-guide.md](../../.agents/rules/architecture-guide.md)):
  - Ensure static event subscriptions in `OnEnable` have matching unsubscriptions in `OnDisable`.
  - Remove any polling from `Update()`.
  - Maintain composition over inheritance (depth $\le 2$).
- **Naming** ([naming-variable-rule.md](../../.agents/rules/naming-variable-rule.md)):
  - Ensure private fields use `_camelCase`.
  - Ensure booleans use modal verbs (`is`, `has`, `can`, etc.).
- **Functions** ([function-rule.md](../../.agents/rules/function-rule.md)):
  - Keep functions focused on a single responsibility with explicit parameters.

### Step 4: Update Centralized `TaskOverview.md`
Immediately upon applying code fixes, update [TaskOverview.md](../ai-docs/TaskOverview.md):
1. Add a new row in the **Completed Tasks Summary Table** indicating `[FIX]` for the task.
2. Append a new section under **Detailed Task Changelog** detailing the fixes applied.

### Step 5: Output Confirmation to User
Output a concise confirmation linking to:
- The modified code files.
- [TaskOverview.md](../ai-docs/TaskOverview.md).
- Prompt the user to run `/review` to re-audit the changes.

---

## 4. `TaskOverview.md` Update Format for Fixes

### Summary Table Row
```markdown
| [FIX] [Task ID / Name] | [Feature/System] | `[ModifiedFile1.cs]` | YYYY-MM-DD |
```

### Detailed Changelog Section
```markdown
### [FIX] [Task ID / Name] — [YYYY-MM-DD HH:MM]
- **Target Files**:
  - [`Assets/Project/Scripts/.../Example.cs`](../../Assets/Project/Scripts/.../Example.cs) ([MODIFIED])
- **Fixes Applied**:
  - Added missing `OnDisable` unsubscription for `GameEventBus.OnCropHarvested` to prevent memory leaks.
  - Renamed private field `currentHealth` to `_currentHealth` to satisfy naming rules.
  - Eliminated polling in `Update()` in favor of event-driven state updates.
```
