param(
    [string]$Output = (Join-Path (Split-Path -Parent $PSScriptRoot) ("TestOutput-" + (Get-Date -Format 'yyyyMMdd-HHmmss')))
)
$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$Output = [System.IO.Path]::GetFullPath($Output)
if (Test-Path $Output) {
    throw "測試輸出資料夾已存在，為避免覆蓋測試資料，請指定新的路徑：$Output"
}

Push-Location $ProjectRoot
try {
    dotnet run -c Release -- self-test $Output
    if ($LASTEXITCODE -ne 0) { throw "自我測試失敗，ExitCode=$LASTEXITCODE" }
    Write-Host "測試報告：$(Join-Path $Output 'SelfTestReport.json')"
}
finally {
    Pop-Location
}
