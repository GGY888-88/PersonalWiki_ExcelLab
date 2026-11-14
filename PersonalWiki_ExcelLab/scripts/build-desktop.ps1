param([string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $ProjectRoot 'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'
dotnet restore $Project
if ($LASTEXITCODE -ne 0) { throw 'Desktop restore failed' }
dotnet build $Project -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Desktop build failed' }
