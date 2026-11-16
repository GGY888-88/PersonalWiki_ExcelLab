param(
    [string]$OutputFolder = 'publish',
    [string]$Runtime = 'win-x64'
)
$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$Output = Join-Path $ProjectRoot $OutputFolder

# Never erase an existing folder: it may contain a previous release or user files.
if (Test-Path $Output) {
    throw "輸出資料夾已存在，為避免覆蓋或刪除資料，請先自行備份並改用新的資料夾名稱：$Output"
}

Push-Location $ProjectRoot
try {
    dotnet publish .\PersonalWiki.ExcelLab.csproj -c Release -r $Runtime --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $Output
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish 失敗，ExitCode=$LASTEXITCODE" }
    Write-Host "Portable 發布檔已輸出到：$Output"
}
finally {
    Pop-Location
}
