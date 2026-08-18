---
name: coding
description: >-
  Coder Agent workflow for BanRaiValley. Use when the user types /coding or provides a task file/instruction to implement. Focuses strictly on executing code following .agent/rule/ without loading long-term design docs, and logs completed work into .agent/ai-docs/TaskOverview.md.
---

# /coding Skill — Coder Agent

Follow the complete workflow defined in [.agent/workflow/coding.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/coding.md):

1. **Parse Input**: Read target task file or direct text instruction.
2. **Implement Code**: Strictly adhere to [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/) (Event-driven, `OnDisable` cleanup, `_camelCase` private fields, modal booleans).
3. **Log Completion**: Append the summary row and detailed changelog entry to [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md).
