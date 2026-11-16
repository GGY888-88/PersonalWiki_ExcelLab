param(
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $ProjectRoot 'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'
$Output = Join-Path $ProjectRoot "publish-desktop-$Runtime"
if (Test-Path $Output) { throw "Output already exists; move it or choose another location: $Output" }
dotnet restore $Project -r $Runtime
if ($LASTEXITCODE -ne 0) { throw 'Desktop restore failed' }
dotnet publish $Project -c $Configuration -r $Runtime --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $Output
if ($LASTEXITCODE -ne 0) { throw 'Desktop publish failed' }
Write-Host "Desktop portable package created: $Output"
