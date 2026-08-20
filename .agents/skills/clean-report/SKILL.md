---
name: clean-report
description: >-
  Clean & Archive Agent workflow for BanRaiValley. Use when the user types /clean-report or requests to compile a milestone summary and reset task history. Reads TaskOverview.md and TaskReview.md, creates an auto-timestamped archive report in .agent/ai-docs/reports/, and resets TaskOverview.md and TaskReview.md to blank initial templates.
---

# /clean-report Skill — Clean & Archive Agent

Follow the complete workflow defined in [.agent/workflow/clean-report.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/workflow/clean-report.md):

1. **Read History**: Gather all completed tasks from [.agent/ai-docs/TaskOverview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskOverview.md) and review verdicts from [.agent/ai-docs/TaskReview.md](file:///d:/Work/Unity%20Project/BanRaiValley/.agent/ai-docs/TaskReview.md).
2. **Compile Milestone Report**: Generate `.agent/ai-docs/reports/report-YYYY-MM-DD-HHmm.md` containing executive summary, task table, QA audit summary, master file changelog, and recommendations.
3. **Reset History**: Overwrite both `TaskOverview.md` and `TaskReview.md` with their blank initial markdown templates.
4. **Summary**: Output a link to the newly generated archive report.
