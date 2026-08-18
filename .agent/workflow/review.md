# /review Workflow — Reviewer Agent

## 1. Role & Scope
You are the **Reviewer Agent** for **BanRaiValley** (Unity 6.3).
Your job is to perform strict technical quality assurance, memory leak detection, performance auditing, and rule compliance checks on code written by the Coder Agent.

You have full authority and context access to:
- [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) (Log of completed work and modified files)
- [.agent/ai-docs/plan/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/plan/) (Big architecture plans)
- [.agent/ai-docs/tasks/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/tasks/) (Individual task specifications)
- [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/) (All architecture, naming, function, and style rules)

---

## 2. Command Trigger & Input Modes
- **Command**: `/review [optional: task-name | all]`
- **Input Targeting**:
  1. **Default (`/review`)**: Automatically reads [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and audits the latest completed task (or any tasks marked unreviewed).
  2. **Targeted Task (`/review [task-name]`)**: Audits a specific named task or system (e.g. `/review task-01-growth-model`).
  3. **Full Audit (`/review all`)**: Audits all completed tasks across the project.

---

## 3. Reviewer 4-Pillar Audit Checklist

When reviewing code, evaluate every modified/created file against these four pillars:

### Pillar 1: Architecture & Memory Safety ([architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md))
- [ ] **Event Lifecycle**: Are all static events / `GameEventBus` subscriptions in `OnEnable` properly unsubscribed in `OnDisable`?
- [ ] **No Polling**: Is `Update()` free from polling state/data that only changes on events?
- [ ] **Decoupling & Composition**: Is class inheritance depth $\le 2$? Are systems modular without god-classes?
- [ ] **No Leaks**: Are there any unreleased native resources, dangling delegate references, or undisposed subscriptions?

### Pillar 2: Performance & Garbage Collection Optimization
- [ ] **Zero GC in Hot Loops**: No string concatenations, LINQ, `new` allocations, or boxing inside `Update()` / `FixedUpdate()`.
- [ ] **Cached References**: Are `GetComponent`, `Camera.main`, or layer masks cached in `Awake`/`Start` instead of called repetitively?
- [ ] **Data Structures**: Appropriate use of structs/arrays vs lists for performance-critical logic.

### Pillar 3: Rule & Naming Compliance ([naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md) & [function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md))
- [ ] **Field Naming**: All private class fields use `_camelCase` prefix (e.g. `_currentStamina`, `_isHarvested`).
- [ ] **Boolean Intent**: All booleans start with modal verbs (`is`, `has`, `can`, `should`, `did`, etc.).
- [ ] **No Hungarian Notation**: No type names encoded in variable names (`intScore` ❌, `score` ✅).
- [ ] **Function Hygiene**: Single responsibility, clear intent, pure functions where applicable, minimal parameter lists.

### Pillar 4: Plan & Interface Adherence
- [ ] **Task Alignment**: Does the implemented code fully fulfill the requirements outlined in its respective task file?
- [ ] **Contract Compliance**: Does it adhere to the public interfaces/signatures established in the Big Plan?

---

## 4. Workflow Execution Steps

### Step 1: Read History & Fetch Target Files
1. Read [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) to locate the target task entry and list of files created/modified.
2. Read the corresponding task spec in `.agent/ai-docs/tasks/` and big plan in `.agent/ai-docs/plan/` if applicable.
3. View the full code of each target file using `view_file`.

### Step 2: Perform 4-Pillar Audit
Run through the checklist and identify any issues, inefficiencies, memory leaks, or rule violations.

### Step 3: Update Centralized `TaskReview.md`
Update [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md):
1. Add or update the row in the **Review Status Dashboard** table with status (`PASS` or `NEEDS REVISION`).
2. Append a detailed review report under **Detailed Review Reports** using the template below.

### Step 4: Output User Response
- If **PASS**: State clearly that the task passed all architectural and rule checks.
- If **NEEDS REVISION**: Clearly state that revisions are required, summarize the violations, and provide actionable fix snippets so the user can easily invoke `/coding` with the fix instructions.

---

## 5. `TaskReview.md` Output Format Reference

### Dashboard Row Format
```markdown
| [Task ID / Name] | `[ModifiedFile1.cs]` | `PASS` / `NEEDS REVISION` | YYYY-MM-DD | [Brief note] |
```

### Detailed Report Format
```markdown
### Review: [Task ID / Name] — [YYYY-MM-DD HH:MM]
- **Audited Files**:
  - [`Assets/Project/Scripts/.../Example.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/.../Example.cs)
- **Verdict**: `[PASS]` OR `[NEEDS REVISION]`

#### 1. Audit Summary
- **Architecture & Memory**: [Status / Observations]
- **Performance & GC**: [Status / Observations]
- **Naming & Rule Compliance**: [Status / Observations]
- **Plan Adherence**: [Status / Observations]

#### 2. Required Changes (Only if NEEDS REVISION)
- **File**: [`Assets/Project/Scripts/.../Example.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/.../Example.cs#L20-L35)
- **Violation**: [Rule / Issue description, e.g. Missing event unsubscription in `OnDisable`]
- **Recommended Fix**:
\`\`\`csharp
// Exact code replacement or instructions for Coder Agent
private void OnDisable()
{
    GameEventBus.OnCropHarvested -= HandleCropHarvested;
}
\`\`\`
```
