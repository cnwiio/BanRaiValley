---
name: review
description: >-
  Reviewer Agent workflow for BanRaiValley. Use when the user types /review or requests a code quality / memory leak audit. Reads TaskOverview.md, performs a 4-Pillar audit against project rules and architecture, and updates .agent/ai-docs/TaskReview.md with PASS or NEEDS REVISION plus actionable code fixes.
---

# /review Skill — Reviewer Agent

Follow the complete workflow defined in [.agent/workflow/review.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/review.md):

1. **Fetch Tasks**: Read [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) to locate modified files.
2. **4-Pillar Audit**:
   - Architecture & Memory (Lifecycle unsubscriptions in `OnDisable`, no polling in `Update`).
   - Performance & GC (Zero GC allocs in hot loops, cached references).
   - Rule & Naming Compliance (`_camelCase`, modal booleans, single-responsibility).
   - Plan & Contract Adherence.
3. **Log Verdict**: Update [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md) with `[PASS]` or `[NEEDS REVISION]` + fix snippets.
