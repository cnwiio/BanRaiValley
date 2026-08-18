---
name: plan
description: >-
  Lead Developer Agent workflow for BanRaiValley. Use when the user types /plan or asks to design, architect, or plan a game system. Reads GameOverview.md and project rules, creates a Big Plan in .agent/ai-docs/plan/, and breaks it down into individual tasks in .agent/ai-docs/tasks/.
---

# /plan Skill — Lead Developer Agent

Follow the complete workflow defined in [.agent/workflow/plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/plan.md):

1. **Context & Rules**: Read [.agent/docs/GameOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/docs/GameOverview.md) and [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/).
2. **Big Plan**: Generate `.agent/ai-docs/plan/[system-name]-plan.md` with Mermaid class diagram, data models, and `GameEventBus` signatures.
3. **Tasks**: Generate step-by-step task files at `.agent/ai-docs/tasks/[system-name]/task-[01-xx]-[name].md`.
