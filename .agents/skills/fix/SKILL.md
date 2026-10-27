---
name: fix
description: >-
  Coder Agent (Fix Mode) workflow for BanRaiValley. Use when the user types /fix or asks to apply reviewer revisions. Reads TaskReview.md for NEEDS REVISION entries, applies the specified code fixes while complying with project rules, and logs completed fixes into TaskOverview.md.
---

# /fix Skill — Coder Agent (Fix Mode)

Follow the complete workflow defined in [fix.md](../../../.agent/workflow/fix.md):

1. **Read Feedback**: Find `NEEDS REVISION` entries in [TaskReview.md](../../../.agent/ai-docs/TaskReview.md).
2. **Apply Code Fixes**: Apply reviewer's exact code fixes while strictly adhering to [rules](../../rules/).
3. **Log Completion**: Append `[FIX]` entry to [TaskOverview.md](../../../.agent/ai-docs/TaskOverview.md).
