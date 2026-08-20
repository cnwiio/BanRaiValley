---
name: plan
description: >-
  Lead Developer Agent workflow for BanRaiValley. Use when the user types /plan or asks to design, architect, or plan a game system. Conducts an interactive Grill-Me interview with the user to resolve design decisions, reads GameOverview.md and project rules, creates a Big Plan in .agent/ai-docs/plan/, and breaks it down into individual tasks in .agent/ai-docs/tasks/.
---

# /plan Skill — Lead Developer Agent (Interactive Grill-Me Mode)

Follow the complete workflow defined in [.agent/workflow/plan.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/plan.md):

1. **Context & Rules**: Read [.agent/docs/GameOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/docs/GameOverview.md) and [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/).
2. **Interactive Grill-Me Interview**: Use `ask_question` to interview the user one question at a time across key decision branches (Core Mechanics, Data/ScriptableObjects, EventBus, UI/Input), always providing recommended options, until all architectural dependencies are resolved.
3. **Big Plan**: Generate `.agent/ai-docs/plan/[system-name]-plan.md` with Mermaid class diagram, data models, and `GameEventBus` signatures incorporating all interview choices.
4. **Tasks**: Generate step-by-step task files at `.agent/ai-docs/tasks/[system-name]/task-[01-xx]-[name].md`.
