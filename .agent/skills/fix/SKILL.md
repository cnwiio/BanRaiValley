---
name: fix
description: >-
  Coder Agent (Fix Mode) workflow for BanRaiValley. Use when the user types /fix or asks to apply reviewer revisions. Reads TaskReview.md for NEEDS REVISION entries, applies the specified code fixes while complying with project rules, and logs completed fixes into .agent/ai-docs/TaskOverview.md.
---

# /fix Skill — Coder Agent (Fix Mode)

Follow the complete workflow defined in [.agent/workflow/fix.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/fix.md):

1. **Read Feedback**: Find `NEEDS REVISION` entries in [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md).
2. **Apply Code Fixes**: Apply reviewer's exact code fixes while strictly adhering to [.agent/rule/](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/rule/).
3. **Log Completion**: Append `[FIX]` entry to [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md).
