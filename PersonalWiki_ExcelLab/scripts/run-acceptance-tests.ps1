param(
    [string]$OutputRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'AcceptanceTestOutput')
)
$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
if (Test-Path $OutputRoot) { throw "Output directory already exists; choose a new path to avoid overwriting: $OutputRoot" }
New-Item -ItemType Directory -Path $OutputRoot | Out-Null
$Passed = 0
$Failed = 0
$Results = [System.Collections.Generic.List[object]]::new()
function Run-Check([string]$Name, [scriptblock]$Action) {
    try {
        & $Action
        $script:Passed++
        $script:Results.Add([pscustomobject]@{ Test=$Name; Result='PASS'; Detail='As expected' })
        Write-Host "[PASS] $Name" -ForegroundColor Green
    } catch {
        $script:Failed++
        $script:Results.Add([pscustomobject]@{ Test=$Name; Result='FAIL'; Detail=$_.Exception.Message })
        Write-Host "[FAIL] $Name - $($_.Exception.Message)" -ForegroundColor Red
    }
}
function Invoke-Cli([string[]]$CliArgs, [int]$ExpectedExit = 0) {
    Push-Location $ProjectRoot
    try { $output = & dotnet run -c Release --no-build -- @CliArgs; $actual=$LASTEXITCODE }
    finally { Pop-Location }
    if ($actual -ne $ExpectedExit) { throw "Command exit code=$actual; expected=$ExpectedExit; command: $($CliArgs -join ' ')" }
    return ($output -join [Environment]::NewLine)
}

Push-Location $ProjectRoot
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed' }
    dotnet build -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }
} finally { Pop-Location }

$dbA = Join-Path $OutputRoot 'test-a.db'
$dbB = Join-Path $OutputRoot 'test-b.db'
$backup = Join-Path $OutputRoot 'test-a-backup.db'
$restored = Join-Path $OutputRoot 'test-a-restored.db'
$protectedTarget = Join-Path $OutputRoot 'must-not-overwrite.db'
$corruptDatabase = Join-Path $OutputRoot 'invalid-source.db'
$failedRestoreTarget = Join-Path $OutputRoot 'must-not-remain.db'
$fixture = Join-Path $OutputRoot 'nullable-guard-fixture.xlsx'
$snapshot = Join-Path $OutputRoot 'schema-snapshot.json'
$integrity = Join-Path $OutputRoot 'integrity-report.json'
$schemaReport = Join-Path $OutputRoot 'schema-check.json'
$entryAudit = Join-Path $OutputRoot 'entry-audit.json'
$noteAudit = Join-Path $OutputRoot 'note-audit.json'
$fieldAudit = Join-Path $OutputRoot 'field-audit.json'
$batchAudit = Join-Path $OutputRoot 'import-batch-audit.json'
$entryInventory = Join-Path $OutputRoot 'entry-inventory-report.json'
$tagUsageReport = Join-Path $OutputRoot 'tag-usage-report.json'
$fieldUsageReport = Join-Path $OutputRoot 'field-usage-report.json'
$noteUsageReport = Join-Path $OutputRoot 'note-usage-report.json'
$sourceTraceAudit = Join-Path $OutputRoot 'source-trace-audit.json'
$relationshipAudit = Join-Path $OutputRoot 'relationship-audit.json'
$tagAudit = Join-Path $OutputRoot 'tag-audit.json'
$imageDuplicateReport = Join-Path $OutputRoot 'image-duplicate-report.json'
$imageInventoryReport = Join-Path $OutputRoot 'image-inventory-report.json'
$storageReport = Join-Path $OutputRoot 'storage-report.json'
$vacuumSource = Join-Path $OutputRoot 'vacuum-source.db'
$vacuumCopy = Join-Path $OutputRoot 'vacuum-copy.db'
$diagnosticBaseline = Join-Path $OutputRoot 'diagnostic-baseline.json'
$diagnosticCurrent = Join-Path $OutputRoot 'diagnostic-current.json'
$diagnosticCompare = Join-Path $OutputRoot 'diagnostic-compare.json'

Run-Check 'CLI help starts' { Invoke-Cli @('help') }
Run-Check 'Self-test includes passing snapshot and safe-output safety checks' { $selfTestRoot=Join-Path $OutputRoot 'SelfTest'; Invoke-Cli @('self-test', $selfTestRoot) | Out-Null; $selfTest=Get-Content -Raw (Join-Path $selfTestRoot 'SelfTestReport.json') | ConvertFrom-Json; if (-not $selfTest.SelfTestPassed -or -not $selfTest.SnapshotSafety.Passed -or -not $selfTest.SafeFileOutputSafety.Passed) { throw 'Self-test safety assertions did not pass' } }
Run-Check 'Create nullable-guard fixture' { Invoke-Cli @('fixture', $fixture) }
$previewOutput = Join-Path $OutputRoot 'import-preview.json'
$qualityOutput = Join-Path $OutputRoot 'import-quality.json'
$mappingOutput = Join-Path $OutputRoot 'mapping-template.json'
$rulesOutput = Join-Path $OutputRoot 'clean-rules-template.json'
$dryRunOutput = Join-Path $OutputRoot 'import-dry-run.json'
Run-Check 'Import preview report refuses overwrite' { Invoke-Cli @('import-preview', $fixture, '--json', $previewOutput) | Out-Null; $before=(Get-FileHash $previewOutput -Algorithm SHA256).Hash; Invoke-Cli @('import-preview', $fixture, '--json', $previewOutput) 1 | Out-Null; if ($before -ne (Get-FileHash $previewOutput -Algorithm SHA256).Hash) { throw 'Import preview report was modified' } }
Run-Check 'Import quality report refuses overwrite' { Invoke-Cli @('import-quality', $fixture, $qualityOutput) | Out-Null; $before=(Get-FileHash $qualityOutput -Algorithm SHA256).Hash; Invoke-Cli @('import-quality', $fixture, $qualityOutput) 1 | Out-Null; if ($before -ne (Get-FileHash $qualityOutput -Algorithm SHA256).Hash) { throw 'Import quality report was modified' } }
Run-Check 'Mapping template refuses overwrite' { Invoke-Cli @('mapping-template', $fixture, $mappingOutput) | Out-Null; $before=(Get-FileHash $mappingOutput -Algorithm SHA256).Hash; Invoke-Cli @('mapping-template', $fixture, $mappingOutput) 1 | Out-Null; if ($before -ne (Get-FileHash $mappingOutput -Algorithm SHA256).Hash) { throw 'Mapping template was modified' } }
Run-Check 'Create isolated all-ignored mapping for cleaning rules test' { $mappingDoc=Get-Content -Raw $mappingOutput | ConvertFrom-Json; foreach($sheet in $mappingDoc.Sheets) { foreach($column in $sheet.Columns) { $column.Mode='Ignore'; $column.TargetField=$null } }; $mappingDoc | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $mappingOutput -Encoding UTF8; Invoke-Cli @('mapping-check', $fixture, $mappingOutput) | Out-Null }
Run-Check 'Cleaning rules template refuses overwrite' { Invoke-Cli @('clean-rules-template', $fixture, $mappingOutput, $rulesOutput) | Out-Null; $before=(Get-FileHash $rulesOutput -Algorithm SHA256).Hash; Invoke-Cli @('clean-rules-template', $fixture, $mappingOutput, $rulesOutput) 1 | Out-Null; if ($before -ne (Get-FileHash $rulesOutput -Algorithm SHA256).Hash) { throw 'Cleaning rules template was modified' } }
Run-Check 'Dry-run report refuses overwrite' { Invoke-Cli @('import-dry-run', $fixture, $mappingOutput, $rulesOutput, $dryRunOutput) | Out-Null; $before=(Get-FileHash $dryRunOutput -Algorithm SHA256).Hash; Invoke-Cli @('import-dry-run', $fixture, $mappingOutput, $rulesOutput, $dryRunOutput) 1 | Out-Null; if ($before -ne (Get-FileHash $dryRunOutput -Algorithm SHA256).Hash) { throw 'Dry-run report was modified' } }
Run-Check 'Safe Excel import uses verified snapshots and preserves original source trace' {
    $safeMapping = Join-Path $OutputRoot 'safe-import-mapping.json'
    $safeRules = Join-Path $OutputRoot 'safe-import-rules.json'
    $safeDb = Join-Path $OutputRoot 'safe-import.db'
    $safeBackup = Join-Path $OutputRoot 'safe-import-backup.db'
    Invoke-Cli @('mapping-template', $fixture, $safeMapping) | Out-Null
    $mappingDoc = Get-Content -Raw $safeMapping | ConvertFrom-Json
    foreach ($sheet in $mappingDoc.Sheets) {
        for ($i = 0; $i -lt $sheet.Columns.Count; $i++) {
            if ($i -eq 0) { $sheet.Columns[$i].Mode = 'Title'; $sheet.Columns[$i].TargetField = $null }
            elseif ($i -eq 1) { $sheet.Columns[$i].Mode = 'Content'; $sheet.Columns[$i].TargetField = $null }
            else { $sheet.Columns[$i].Mode = 'Ignore'; $sheet.Columns[$i].TargetField = $null }
        }
    }
    $mappingDoc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $safeMapping -Encoding UTF8
    Invoke-Cli @('clean-rules-template', $fixture, $safeMapping, $safeRules) | Out-Null
    $rulesDoc = Get-Content -Raw $safeRules | ConvertFrom-Json
    foreach ($sheet in $rulesDoc.Sheets) { foreach ($rule in $sheet.Rules) { $rule.Enabled = $false } }
    $rulesDoc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $safeRules -Encoding UTF8
    Invoke-Cli @('db-init', $safeDb) | Out-Null
    $result = (Invoke-Cli @('excel-import-db-safe', $safeDb, $fixture, $safeMapping, $safeRules, $safeBackup) | ConvertFrom-Json)
    if (-not $result.BatchVerified -or [string]::IsNullOrWhiteSpace([string]$result.BatchId)) { throw 'Safe import did not complete and verify' }; if (-not (@($result.Stages) -match 'quick_check\(1\).*foreign_key_check')) { throw 'Pre-commit SQLite transaction checks were not reported' }
    $batches = (Invoke-Cli @('excel-import-batches', $safeDb) | ConvertFrom-Json)
    $batch = @($batches | Where-Object { $_.Id -eq $result.BatchId }) | Select-Object -First 1
    if ($null -eq $batch) { throw 'Imported batch was not found' }
    if ([System.IO.Path]::GetFullPath([string]$batch.SourceFile) -ne [System.IO.Path]::GetFullPath($fixture)) { throw 'Import source trace contains a temporary snapshot path instead of the original workbook path' }
    Invoke-Cli @('excel-import-verify', $safeDb, [string]$result.BatchId) | Out-Null
}

Run-Check 'Inspect nullable-guard fixture' { Invoke-Cli @('inspect', $fixture) }
Run-Check 'Create test database A' { Invoke-Cli @('db-init', $dbA) }
Run-Check 'Create isolated test entry' { Invoke-Cli @('entry-add', $dbA, 'Backup regression fixture', 'Test', 'Generated only for automated acceptance') }
Run-Check 'Search across notes and custom fields (2.0 regression)' {
    $created = (Invoke-Cli @('entry-add', $dbA, 'Search coverage fixture', 'Test', 'Release regression entry') | ConvertFrom-Json)
    $entryId = [string]$created.Id
    if ([string]::IsNullOrWhiteSpace($entryId)) { throw 'Entry ID was not returned' }
    Invoke-Cli @('field-set', $dbA, $entryId, 'release_token', 'Release token', 'FIELD_ONLY_20_TOKEN') | Out-Null
    Invoke-Cli @('note-add', $dbA, $entryId, 'NOTE_ONLY_20_TOKEN') | Out-Null
    $fieldSearch = (Invoke-Cli @('entry-query', $dbA, 'FIELD_ONLY_20_TOKEN') | ConvertFrom-Json)
    $noteSearch = (Invoke-Cli @('entry-query', $dbA, 'NOTE_ONLY_20_TOKEN') | ConvertFrom-Json)
    if ($fieldSearch.TotalCount -ne 1 -or $fieldSearch.Items[0].Id -ne $entryId) { throw 'Custom-field content search did not return the expected entry' }
    if ($noteSearch.TotalCount -ne 1 -or $noteSearch.Items[0].Id -ne $entryId) { throw 'Note content search did not return the expected entry' }
    $paged = (Invoke-Cli @('entry-query', $dbA, '--page', '1', '--page-size', '1') | ConvertFrom-Json)
    if ($paged.Page -ne 1 -or $paged.PageSize -ne 1 -or $paged.Items.Count -gt 1) { throw 'Paged query metadata or result limit is invalid' }
}
Run-Check 'Check full-text index consistency' { Invoke-Cli @('db-index-check', $dbA) }
Run-Check 'Rebuild full-text index transactionally' { Invoke-Cli @('db-index-rebuild', $dbA); Invoke-Cli @('db-index-check', $dbA) }
Run-Check 'Create consistent database backup' { Invoke-Cli @('db-backup', $dbA, $backup); if (-not (Test-Path $backup)) { throw 'Backup file was not created' } }
Run-Check 'Verify backup health' { Invoke-Cli @('db-check', $backup) }
Run-Check 'Restore backup and verify table counts' { Invoke-Cli @('db-recovery-restore-verify', $backup, $restored); if (-not (Test-Path $restored)) { throw 'Verified restore was not created' } }
Run-Check 'Compare source and restored database content' { Invoke-Cli @('db-content-compare', $dbA, $restored) }
Run-Check 'Refuse to overwrite existing backup target' { Set-Content -LiteralPath $protectedTarget -Value 'KEEP-ME' -NoNewline; $before=(Get-FileHash $protectedTarget -Algorithm SHA256).Hash; Invoke-Cli @('db-backup', $dbA, $protectedTarget) 1; $after=(Get-FileHash $protectedTarget -Algorithm SHA256).Hash; if ($before -ne $after) { throw 'Existing target contents changed' } }
Run-Check 'Refuse to overwrite existing restore target' { Invoke-Cli @('db-restore', $backup, $protectedTarget) 1; if ((Get-Content -Raw $protectedTarget) -ne 'KEEP-ME') { throw 'Existing target contents changed' } }
Run-Check 'Refuse source-equals-backup path' { Invoke-Cli @('db-backup', $dbA, $dbA) 1 }
Run-Check 'Refuse source-equals-restore path' { Invoke-Cli @('db-restore', $backup, $backup) 1 }
Run-Check 'Reject invalid SQLite source without leaving staging files' { Set-Content -LiteralPath $corruptDatabase -Value 'not-a-sqlite-database'; Invoke-Cli @('db-backup', $corruptDatabase, (Join-Path $OutputRoot 'invalid-backup.db')) 1; $leftovers = @(Get-ChildItem -LiteralPath $OutputRoot -Filter '*.staging' -Force); if ($leftovers.Count -gt 0) { throw 'Staging files were left behind' } }
Run-Check 'Reject invalid restore backup without creating target' { Invoke-Cli @('db-recovery-restore-verify', $corruptDatabase, $failedRestoreTarget) 1; if (Test-Path $failedRestoreTarget) { throw 'Invalid restore target should not exist' } }
Run-Check 'Create read-only tag usage report' { Invoke-Cli @('db-tag-usage-report', $dbA, $tagUsageReport); if (-not (Test-Path $tagUsageReport)) { throw 'Tag usage report was not created' } }
Run-Check 'Create read-only field usage report' { Invoke-Cli @('db-field-usage-report', $dbA, $fieldUsageReport); if (-not (Test-Path $fieldUsageReport)) { throw 'Field usage report was not created' } }
Run-Check 'Create read-only note usage report' { Invoke-Cli @('db-note-usage-report', $dbA, $noteUsageReport); if (-not (Test-Path $noteUsageReport)) { throw 'Note usage report was not created' } }
Run-Check 'Create read-only relationship audit report' { Invoke-Cli @('db-relationship-audit', $dbA, $relationshipAudit); if (-not (Test-Path $relationshipAudit)) { throw 'Relationship audit report was not created' } }
Run-Check 'Create read-only batch source reconciliation report' { $batchSource = Join-Path $OutputRoot 'batch-source-reconciliation.json'; Invoke-Cli @('db-batch-source-reconciliation', $dbA, $batchSource); if (-not (Test-Path $batchSource)) { throw 'Batch source reconciliation report was not created' } }
Run-Check 'Create read-only image file integrity audit report' { $imageIntegrity = Join-Path $OutputRoot 'image-file-integrity.json'; Invoke-Cli @('db-image-file-integrity-audit', $dbA, $imageIntegrity); if (-not (Test-Path $imageIntegrity)) { throw 'Image file integrity report was not created' } }
Run-Check 'Create read-only source trace audit report' { Invoke-Cli @('db-source-trace-audit', $dbA, $sourceTraceAudit); if (-not (Test-Path $sourceTraceAudit)) { throw 'Source trace audit report was not created' } }
Run-Check 'Create read-only entry inventory report' { Invoke-Cli @('db-entry-inventory-report', $dbA, $entryInventory); if (-not (Test-Path $entryInventory)) { throw 'Entry inventory report was not created' } }
Run-Check 'Create read-only import batch audit report' { Invoke-Cli @('db-import-batch-audit', $dbA, $batchAudit); if (-not (Test-Path $batchAudit)) { throw 'Import batch audit report was not created' } }
Run-Check 'Create read-only tag audit report' { Invoke-Cli @('db-tag-audit', $dbA, $tagAudit); if (-not (Test-Path $tagAudit)) { throw 'Tag audit report was not created' } }
Run-Check 'Create read-only field audit report' { Invoke-Cli @('db-field-audit', $dbA, $fieldAudit); if (-not (Test-Path $fieldAudit)) { throw 'Field audit report was not created' } }
Run-Check 'Create read-only note audit report' { Invoke-Cli @('db-note-audit', $dbA, $noteAudit); if (-not (Test-Path $noteAudit)) { throw 'Note audit report was not created' } }
Run-Check 'Create read-only entry audit report' { Invoke-Cli @('db-entry-audit', $dbA, $entryAudit) }
Run-Check 'Create read-only image duplicate report' { Invoke-Cli @('db-image-duplicate-report', $dbA, $imageDuplicateReport) }
Run-Check 'Create read-only image inventory report' { Invoke-Cli @('db-image-inventory-report', $dbA, $imageInventoryReport); if (-not (Test-Path $imageInventoryReport)) { throw 'Image inventory report was not created' } }
Run-Check 'Create read-only database storage report' { Invoke-Cli @('db-storage-report', $dbA, $storageReport) }
Run-Check 'Create consolidated read-only database diagnostic report' {
    $diagnostic = Join-Path $OutputRoot 'database-diagnostic.json'
    $before = (Get-FileHash -LiteralPath $dbA -Algorithm SHA256).Hash
    $result = (Invoke-Cli @('db-diagnostic-report', $dbA, $diagnostic) | ConvertFrom-Json)
    $after = (Get-FileHash -LiteralPath $dbA -Algorithm SHA256).Hash
    if (-not $result.IsHealthy -or -not $result.Health.IsHealthy -or -not $result.SearchIndex.IsConsistent) { throw 'Diagnostic report did not confirm a healthy database and index' }
    if ($result.Tables.Count -lt 1 -or $result.Warnings.Count -ne 0 -or $result.Format -ne 'personalwiki-database-diagnostic-v1') { throw 'Diagnostic report is incomplete or has an unexpected format' }
    if ($before -ne $after) { throw 'Read-only diagnostic report modified the source database file' }
    $protectedDiagnostic = Join-Path $OutputRoot 'protected-diagnostic.json'
    Set-Content -LiteralPath $protectedDiagnostic -Value 'KEEP-DIAGNOSTIC' -NoNewline
    $protectedBefore = (Get-FileHash -LiteralPath $protectedDiagnostic -Algorithm SHA256).Hash
    Invoke-Cli @('db-diagnostic-report', $dbA, $protectedDiagnostic) 1 | Out-Null
    $protectedAfter = (Get-FileHash -LiteralPath $protectedDiagnostic -Algorithm SHA256).Hash
    if ($protectedBefore -ne $protectedAfter) { throw 'Existing diagnostic report was modified' }
    $temporaryFiles = @(Get-ChildItem -LiteralPath $OutputRoot -Filter 'database-diagnostic.json.*.tmp' -Force -ErrorAction SilentlyContinue)
    if ($temporaryFiles.Count -ne 0) { throw 'Diagnostic report temporary files were not cleaned up' }
    Invoke-Cli @('db-diagnostic-report', $dbA, $dbA) 1 | Out-Null
}
Run-Check 'Compare two validated diagnostic snapshots without changing inputs' {
    Invoke-Cli @('db-diagnostic-report', $dbA, $diagnosticBaseline) | Out-Null
    Invoke-Cli @('db-diagnostic-report', $dbA, $diagnosticCurrent) | Out-Null
    $beforeA = (Get-FileHash -LiteralPath $diagnosticBaseline -Algorithm SHA256).Hash
    $beforeB = (Get-FileHash -LiteralPath $diagnosticCurrent -Algorithm SHA256).Hash
    $comparison = (Invoke-Cli @('db-diagnostic-compare', $diagnosticBaseline, $diagnosticCurrent, $diagnosticCompare) | ConvertFrom-Json)
    if ($comparison.Format -ne 'personalwiki-database-diagnostic-compare-v1' -or $comparison.HasChanges) { throw 'Identical snapshots should produce a no-change comparison' }
    if ($comparison.TableChanges.Count -ne 0 -or $comparison.AddedTables.Count -ne 0 -or $comparison.RemovedTables.Count -ne 0) { throw 'Identical snapshots reported table differences' }
    if ($beforeA -ne (Get-FileHash -LiteralPath $diagnosticBaseline -Algorithm SHA256).Hash -or $beforeB -ne (Get-FileHash -LiteralPath $diagnosticCurrent -Algorithm SHA256).Hash) { throw 'Comparison modified an input snapshot' }
    Invoke-Cli @('db-diagnostic-compare', $diagnosticBaseline, $diagnosticCurrent, $diagnosticBaseline) 1 | Out-Null
    $protected = Join-Path $OutputRoot 'protected-diagnostic-compare.json'
    Set-Content -LiteralPath $protected -Value 'KEEP-COMPARE' -NoNewline
    $protectedHash = (Get-FileHash -LiteralPath $protected -Algorithm SHA256).Hash
    Invoke-Cli @('db-diagnostic-compare', $diagnosticBaseline, $diagnosticCurrent, $protected) 1 | Out-Null
    if ($protectedHash -ne (Get-FileHash -LiteralPath $protected -Algorithm SHA256).Hash) { throw 'Existing comparison output was modified' }
    $invalid = Join-Path $OutputRoot 'invalid-diagnostic.json'
    Set-Content -LiteralPath $invalid -Value '{invalid-json' -NoNewline
    Invoke-Cli @('db-diagnostic-compare', $invalid, $diagnosticCurrent, (Join-Path $OutputRoot 'invalid-compare.json')) 1 | Out-Null
    $sampleDiff = Join-Path $OutputRoot 'sample-diagnostic-diff.json'
    $sampleResult = (Invoke-Cli @('db-diagnostic-compare', (Join-Path $ProjectRoot 'samples/diagnostic-baseline-sample.json'), (Join-Path $ProjectRoot 'samples/diagnostic-current-sample.json'), $sampleDiff) | ConvertFrom-Json)
    if (-not $sampleResult.HasChanges -or $sampleResult.EntriesDelta -ne 1 -or $sampleResult.AddedTables -notcontains 'tags') { throw 'Sample diagnostic comparison did not detect the expected entry and added-table changes' }
    $entryChange = @($sampleResult.TableChanges | Where-Object { $_.Table -eq 'entries' })
    if ($entryChange.Count -ne 1 -or $entryChange[0].BeforeRows -ne 10 -or $entryChange[0].AfterRows -ne 11 -or $entryChange[0].Delta -ne 1) { throw 'Sample diagnostic comparison did not calculate the entries table delta' }

    $removedDiff = Join-Path $OutputRoot 'sample-diagnostic-removed-diff.json'
    $removedResult = (Invoke-Cli @('db-diagnostic-compare', (Join-Path $ProjectRoot 'samples/diagnostic-removed-table-baseline-sample.json'), (Join-Path $ProjectRoot 'samples/diagnostic-removed-table-current-sample.json'), $removedDiff) | ConvertFrom-Json)
    if (-not $removedResult.HasChanges -or $removedResult.RemovedTables -notcontains 'notes' -or $removedResult.AddedTables.Count -ne 0) { throw 'Diagnostic comparison did not detect a removed table' }
    $decreasedEntries = @($removedResult.TableChanges | Where-Object { $_.Table -eq 'entries' })
    if ($decreasedEntries.Count -ne 1 -or $decreasedEntries[0].BeforeRows -ne 10 -or $decreasedEntries[0].AfterRows -ne 8 -or $decreasedEntries[0].Delta -ne -2) { throw 'Diagnostic comparison did not calculate a negative table row delta' }

    $degradedDiff = Join-Path $OutputRoot 'sample-diagnostic-degraded-diff.json'
    $degradedResult = (Invoke-Cli @('db-diagnostic-compare', (Join-Path $ProjectRoot 'samples/diagnostic-baseline-sample.json'), (Join-Path $ProjectRoot 'samples/diagnostic-schema-unhealthy-sample.json'), $degradedDiff) | ConvertFrom-Json)
    if (-not $degradedResult.HasChanges -or -not $degradedResult.SchemaVersionChanged -or $degradedResult.SchemaVersionBefore -ne 1 -or $degradedResult.SchemaVersionAfter -ne 2) { throw 'Diagnostic comparison did not detect a Schema version change' }
    if ($degradedResult.HealthyAfter -or $degradedResult.QuickCheckPassedAfter -or $degradedResult.ForeignKeyIssuesAfter -ne 1 -or $degradedResult.SearchIndexConsistentAfter) { throw 'Diagnostic comparison did not preserve unhealthy database and index status' }
    if ($degradedResult.SameDatabasePath) { throw 'Diagnostic comparison did not detect a changed database path' }

    $duplicateTables = Join-Path $ProjectRoot 'samples/diagnostic-duplicate-table-invalid-sample.json'
    Invoke-Cli @('db-diagnostic-compare', $duplicateTables, $diagnosticCurrent, (Join-Path $OutputRoot 'duplicate-table-compare.json')) 1 | Out-Null

    foreach ($invalidSampleName in @(
        'diagnostic-invalid-page-metrics-sample.json',
        'diagnostic-invalid-search-index-sample.json',
        'diagnostic-invalid-health-count-sample.json')) {
        $invalidSample = Join-Path $ProjectRoot ('samples/' + $invalidSampleName)
        $invalidOutput = Join-Path $OutputRoot ($invalidSampleName + '.report.json')
        Invoke-Cli @('db-diagnostic-compare', $invalidSample, $diagnosticCurrent, $invalidOutput) 1 | Out-Null
        if (Test-Path -LiteralPath $invalidOutput) { throw "Invalid diagnostic sample unexpectedly created a report: $invalidSampleName" }
    }

    $warningDiff = Join-Path $OutputRoot 'sample-diagnostic-warning-diff.json'
    $warningBaseline = Join-Path $ProjectRoot 'samples/diagnostic-warning-baseline-sample.json'
    $warningCurrent = Join-Path $ProjectRoot 'samples/diagnostic-warning-current-sample.json'
    $warningBaselineHash = (Get-FileHash -LiteralPath $warningBaseline -Algorithm SHA256).Hash
    $warningCurrentHash = (Get-FileHash -LiteralPath $warningCurrent -Algorithm SHA256).Hash
    $warningResult = (Invoke-Cli @('db-diagnostic-compare', $warningBaseline, $warningCurrent, $warningDiff) | ConvertFrom-Json)
    if (-not $warningResult.HasChanges -or $warningResult.AddedWarnings.Count -ne 1 -or $warningResult.RemovedWarnings.Count -ne 1) { throw 'Diagnostic comparison did not detect a warning-only change' }
    if (($warningResult.WarningsAfter -notcontains '示範診斷警告：新警告內容。') -or ($warningResult.WarningsBefore -notcontains '示範診斷警告：舊警告內容。')) { throw 'Diagnostic comparison did not preserve warning details' }
    if ($warningBaselineHash -ne (Get-FileHash -LiteralPath $warningBaseline -Algorithm SHA256).Hash -or $warningCurrentHash -ne (Get-FileHash -LiteralPath $warningCurrent -Algorithm SHA256).Hash) { throw 'Warning comparison modified an input snapshot' }
}
Run-Check 'Prepare isolated database for vacuum-copy' { Copy-Item $dbA $vacuumSource }
Run-Check 'Create optimized database copy without changing source' { $before=(Get-FileHash $vacuumSource -Algorithm SHA256).Hash; Invoke-Cli @('db-vacuum-copy', $vacuumSource, $vacuumCopy); $after=(Get-FileHash $vacuumSource -Algorithm SHA256).Hash; if ($before -ne $after) { throw 'Source database hash changed' }; if (-not (Test-Path $vacuumCopy)) { throw 'Optimized copy was not created' } }
Run-Check 'Copy test database A to B' { Copy-Item $dbA $dbB }
Run-Check 'Identical database content comparison' { Invoke-Cli @('db-content-compare', $dbA, $dbB) }
Run-Check 'Identical database schema comparison' { Invoke-Cli @('db-schema-compare', $dbA, $dbB) }
Run-Check 'Create schema snapshot' { Invoke-Cli @('db-schema-snapshot', $dbA, $snapshot) }
Run-Check 'Refuse to overwrite existing package snapshot manifest' { $protectedSnapshot = Join-Path $OutputRoot 'protected-package-snapshot.json'; Set-Content -LiteralPath $protectedSnapshot -Value 'KEEP-PACKAGE-MANIFEST' -NoNewline; $before=(Get-FileHash $protectedSnapshot -Algorithm SHA256).Hash; Invoke-Cli @('snapshot', $fixture, $protectedSnapshot) 1; $after=(Get-FileHash $protectedSnapshot -Algorithm SHA256).Hash; if ($before -ne $after) { throw 'Existing package snapshot manifest was modified' } }
Run-Check 'Validate schema snapshot' { Invoke-Cli @('db-schema-snapshot-check', $dbA, $snapshot) }
Run-Check 'Create schema snapshot JSON report' { Invoke-Cli @('db-schema-snapshot-check-json', $dbA, $snapshot, $schemaReport) }
Run-Check 'Create database integrity report' { Invoke-Cli @('db-integrity-report', $dbA, $integrity) }
Run-Check 'Validate database integrity report' { Invoke-Cli @('db-integrity-report-check', $dbA, $integrity) }
Run-Check 'Missing database returns an error (negative test)' { Invoke-Cli @('db-schema-snapshot-check', (Join-Path $OutputRoot 'missing.db'), $snapshot) 1 }

$summary = [pscustomobject]@{
    Version = '3.0.0'
    TestPackRevision = '3.0.0-streaming-json-release'
    ExecutedAt = (Get-Date).ToString('o')
    ProjectRoot = $ProjectRoot
    OutputRoot = $OutputRoot
    Passed = $Passed
    Failed = $Failed
    Results = $Results
}
$summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 (Join-Path $OutputRoot 'AcceptanceTestReport.json')
Write-Host "`nTest results: PASS=$Passed FAIL=$Failed"
Write-Host "Report: $(Join-Path $OutputRoot 'AcceptanceTestReport.json')"
if ($Failed -gt 0) { exit 1 }
