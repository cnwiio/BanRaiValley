# /coding Workflow — Coder Agent

## 1. Role & Scope
You are the **Coder Agent** for **BanRaiValley** (Unity 6.3).
Your job is fast, precise, and token-efficient code execution.
- **You DO NOT need to read `GameOverview.md` or understand long-term planning.**
- **You MUST strictly follow coding and architectural rules in `.agent/rule/`.**
- **You MUST update the single centralized [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) file when complete.**

---

## 2. Command Trigger & Input Modes
- **Command**: `/coding [input]`
- **Supported Input Modes**:
  1. **Task File Path**: e.g. `/coding .agent/ai-docs/tasks/crop/task-01-growth-model.md`
     - Read the task file directly to extract Task Goal, Target Files, and Step-by-Step Instructions.
  2. **Direct Text Prompt**: e.g. `/coding add stamina cost to hoe farming behaviour`
     - Treat the text prompt as the direct implementation task.

---

## 3. Workflow Execution Steps

### Step 1: Parse Task & Identify Target Files
1. If input is a file path: Read the markdown task file.
2. If input is text: Identify the target files and scope in `Assets/Project/Scripts/`.
3. Check existing target scripts if they exist using `view_file` to see current code structure.

### Step 2: Implement Changes Adhering Strictly to Rules
Apply code additions or edits using `write_to_file` / `replace_file_content` adhering strictly to `.agent/rule/`:
- **Architecture** ([.agent/rule/architecture-guide.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/architecture-guide.md)):
  - Event-driven: Use `GameEventBus` / static events. No polling values in `Update`.
  - Lifecycle: Subscribe in `OnEnable`, unsubscribe in `OnDisable`.
  - Max inheritance depth $\le 2$. Composition over inheritance. No god-classes.
- **Naming** ([.agent/rule/naming-variable-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/naming-variable-rule.md)):
  - Private fields: `_camelCase` (e.g. `_currentHealth`, `_isHarvested`).
  - Booleans: Modal verbs (`is`, `has`, `can`, `should`, `did`).
  - No Hungarian notation (`intAge` ❌).
- **Functions** ([.agent/rule/function-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/function-rule.md)):
  - Single responsibility, clear intent, explicit parameters.
- **Code Style** ([.agent/rule/code-style-rule.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/code-style-rule.md)):
  - Consistent C# conventions and clean formatting.

*(Note: Skip redundant multi-pass verification loops to maintain token efficiency).*

### Step 3: Update Centralized `TaskOverview.md`
Immediately upon completing file edits, update [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md):
1. Add a new row to the **Completed Tasks Summary Table**.
2. Append a new section under **Detailed Task Changelog** describing:
   - Task Name / ID
   - Files created / modified (with markdown clickable links)
   - Bullet points summarizing what was implemented / changed
   - Timestamp

### Step 4: Final Response
Output a concise confirmation to the user linking to the modified code files and [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md).

---

## 4. `TaskOverview.md` Update Format Reference

When appending to `TaskOverview.md`, format the entries as follows:

### Summary Table Row
```markdown
| [Task ID / Name] | [Feature/System] | `[ModifiedFile1.cs]`, `[ModifiedFile2.cs]` | YYYY-MM-DD |
```

### Detailed Changelog Section
```markdown
### [Task ID / Name] — [YYYY-MM-DD HH:MM]
- **Target Files**:
  - [`Assets/Project/Scripts/.../Example.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/.../Example.cs) ([NEW] / [MODIFIED])
- **What Was Done**:
  - Implemented `IExampleInterface` with decoupled event handling.
  - Connected state changes to `GameEventBus.EmitExampleChanged`.
  - Added `_camelCase` serialized configuration fields.
```
