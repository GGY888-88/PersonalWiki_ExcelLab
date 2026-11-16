# PersonalWiki ExcelLab 4.6.2 — DPI Hotfix build correction

This hotfix corrects compile-time nullable/event-owner issues found when building the 4.6.2 DPI hotfix source on Windows.

## Changes

- `DpiLayout.EnsureTextFits` and `DpiLayout.FitFlowPanelHeight` now take a `Form` as their DPI event owner. Their callers pass the main WinForms form, which is the type that exposes `DpiChanged` and `Shown`.
- The `ControlAdded` handler now checks the nullable `ControlEventArgs.Control` value before attaching events.
- `EntryQueryService` and `EntryQueryPlanService` normalize the sort argument once before dictionary lookup and subsequent use, preventing nullable dereference warnings while retaining the supported-sort validation behavior.

No database schema, database service, Excel import processing, or import mapping behavior is changed by this correction.

## Verification

- 6 targeted static build-fix checks pass.
- 19 DPI static release checks pass.
- 6 UI regression checks pass.
- 4.6.0 query acceptance contract regression checks pass.
- A .NET build and Windows 300% DPI runtime verification were not run in this environment because the .NET SDK and Windows runtime are unavailable here. Run `dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release` on Windows to confirm compilation, then repeat the 300% UI checks.
