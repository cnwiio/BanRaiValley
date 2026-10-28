# Task 04: Unity CLI Editor Validation Utility

## 1. Task Goal
Create an Editor CLI utility script `BanRaiValleyEditorCLI.cs` and test runner method callable from the command line in batchmode (`-executeMethod BanRaiValley.EditorCLI.VerifyProject`) to enable automated project compilation, asset verification, and build validation without launching the interactive GUI.

## 2. Task Information
- **System**: Tooling & CI / Automation
- **Parent Plan**: [continuous-planting-harvesting-plan.md](../../plan/continuous-planting-harvesting-plan.md)
- **Target File(s)**:
  - `Assets/Project/Editor/BanRaiValleyEditorCLI.cs`
- **Dependencies / Prerequisites**:
  - Tasks 01–03
- **Applicable Rules**:
  - `architecture-guide.md`: Isolated editor utilities under `Editor/` folder.
  - `function-rule.md`: Clear method contracts, exit codes on headless execution.

## 3. What To Do (Step-by-Step Instructions)
1. **Create `BanRaiValleyEditorCLI.cs`**:
   - Location: `Assets/Project/Editor/BanRaiValleyEditorCLI.cs`
   - Namespace: `BanRaiValley`
   - Class: `public static class EditorCLI`
   - Static method: `public static void VerifyProject()`
     - Force asset database refresh: `AssetDatabase.Refresh();`
     - Check for compilation errors via `EditorUtility.scriptCompilationFailed`.
     - Log summary results to `Debug.Log`.
     - Call `EditorApplication.Exit(0)` if successful, or `EditorApplication.Exit(1)` if errors exist.
2. **Execute Validation**:
   - Command:
     ```powershell
     & "C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe" -batchmode -quit -projectPath "." -executeMethod BanRaiValley.EditorCLI.VerifyProject -logFile "Logs/cli-verify.log"
     ```

## 4. Verification & Testing Checklist
- [ ] Script is placed in an `Editor` folder so it doesn't get bundled into player builds.
- [ ] Unity CLI batchmode command runs and exits with code 0.
- [ ] `Logs/cli-verify.log` records successful validation.
