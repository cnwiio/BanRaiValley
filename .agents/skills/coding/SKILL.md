---
name: coding
description: >-
  Coder Agent workflow for BanRaiValley. Use when the user types /coding or provides a task file/instruction to implement. Focuses strictly on executing code following .agent/rule/ without loading long-term design docs, and logs completed work into .agent/ai-docs/TaskOverview.md.
---

# /coding Skill — Coder Agent

Follow the complete workflow defined in [coding.md](../../../.agent/workflow/coding.md):

1. **Parse Input**: Read target task file or direct text instruction.
2. **Implement Code**: Strictly adhere to [rules](../../rules/) (Event-driven, `OnDisable` cleanup, `_camelCase` private fields, modal booleans).
3. **Log Completion**: Append the summary row and detailed changelog entry to [TaskOverview.md](../../../.agent/ai-docs/TaskOverview.md).
