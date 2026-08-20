# /clean-report Workflow — Clean & Archive Agent

## 1. Role & Scope
You are the **Archive & Clean-Up Agent** for **BanRaiValley** (Unity 6.3).
Your job is to read all accumulated task completions and QA reviews, compile a comprehensive milestone report saved to `.agent/ai-docs/reports/`, and reset both [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md) to their blank initial states.

---

## 2. Command Trigger & Usage
- **Command**: `/clean-report`
- **Behavior**:
  1. Reads existing [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md).
  2. Generates an auto-timestamped milestone report at `.agent/ai-docs/reports/report-YYYY-MM-DD-HHmm.md`.
  3. Resets `TaskOverview.md` and `TaskReview.md` to clean empty templates with zero history.
  4. Outputs a summary with a direct link to the newly archived report.

---

## 3. Workflow Execution Steps

### Step 1: Read Active Task & Review History
1. View and parse [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) to gather all completed tasks, modified files, and changelogs.
2. View and parse [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md) to gather all review statuses, audit summaries, and verdicts.

### Step 2: Compile Comprehensive Milestone Report
Create a new file at `.agent/ai-docs/reports/report-[YYYY-MM-DD-HHmm].md` following the **Milestone Report Template** below.
The report must include:
- **1. Executive Summary**: Overall scope, completed subsystems, date range, and task counts.
- **2. Completed Tasks & Features Index**: Aggregated table of all tasks executed during this cycle.
- **3. QA & Review Audit Summary**: Status breakdown of all reviewed tasks (`PASS` / `REVISION`).
- **4. Consolidated Master File Changelog**: Comprehensive registry of all files created and modified (with clickable markdown links).
- **5. Recommendations & Next Steps**: Architectural observations or suggested next priorities.

### Step 3: Reset `TaskOverview.md` to Clean Blank Template
Overwrite [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) with the blank initial template:

```markdown
# Task Overview & Completed Work

This file tracks all completed tasks performed by Coder Agents across the project. Other agents can read this file to understand the current implementation state, modified files, and recent system additions.

---

## Completed Tasks Summary Table

| Task ID / Name | System / Feature | Files Created / Modified | Completed Date |
| :--- | :--- | :--- | :--- |

---

## Detailed Task Changelog

<!-- New completed task entries are appended below chronologically -->
```

### Step 4: Reset `TaskReview.md` to Clean Blank Template
Overwrite [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md) with the blank initial template:

```markdown
# Task Review Dashboard & Code Audits

This document tracks technical reviews and quality assurance audits performed by the **Reviewer Agent**. It validates memory safety, performance, project rules, and architectural integrity across completed tasks.

---

## Review Status Dashboard

| Task ID / Name | Target Files | Status | Review Date | Notes |
| :--- | :--- | :--- | :--- | :--- |

---

## Detailed Review Reports

<!-- Chronological review reports will be recorded below -->
```

### Step 5: Final Response
Output a clean confirmation to the user linking to the archived report file and confirming that `TaskOverview.md` and `TaskReview.md` have been reset to zero.

---

## 4. Milestone Report Template (`.agent/ai-docs/reports/report-YYYY-MM-DD-HHmm.md`)

```markdown
# Milestone Archive Report — [YYYY-MM-DD HH:mm]

## 1. Executive Summary
- **Archive Date**: YYYY-MM-DD HH:mm
- **Total Completed Tasks**: [Count]
- **Total Audited Tasks**: [Count]
- **QA Pass Rate**: [e.g. 100% (7/7 Passed)]
- **Key Systems Implemented**: [List of subsystems]

---

## 2. Completed Tasks Index
| Task ID / Name | System / Feature | Files Modified | Completed Date |
| :--- | :--- | :--- | :--- |
| [Task ID] | [System] | `[Files]` | YYYY-MM-DD |

---

## 3. QA & Review Audit Summary
| Task ID / Name | Status | Review Date | Key Audit Notes |
| :--- | :--- | :--- | :--- |
| [Task ID] | `PASS` | YYYY-MM-DD | [Brief note] |

---

## 4. Consolidated Master File Changelog
### New Files Created
- [`Assets/Project/Scripts/.../FileA.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/.../FileA.cs) — [Short description]

### Existing Files Modified
- [`Assets/Project/Scripts/.../FileB.cs`](file:///d:/Work/Unity%20Project/BanRaiValley/Assets/Project/Scripts/.../FileB.cs) — [Short description]

---

## 5. Detailed Task Changelogs Archive
[Full detailed logs preserved from TaskOverview.md]

---

## 6. Recommendations & Next Steps
- [Actionable recommendations for next development cycle]
```
