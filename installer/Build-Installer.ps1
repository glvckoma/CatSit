[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+(?:\.\d+)?$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$projectPath = Join-Path $repoRoot 'src\CatSit\CatSit.csproj'
$publishPath = Join-Path $repoRoot 'artifacts\installer-publish'
$installerScript = Join-Path $PSScriptRoot 'CatSit.iss'

if (-not $Version) {
    [xml]$project = Get-Content -Raw $projectPath
    $Version = $project.Project.PropertyGroup.Version | Select-Object -First 1
}

if (-not $Version) {
    throw 'CatSit.csproj does not define a version. Pass one with -Version.'
}

$innoCandidates = @(
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

$innoCompiler = $innoCandidates | Select-Object -First 1
if (-not $innoCompiler) {
    $innoCommand = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
    if ($innoCommand) {
        $innoCompiler = $innoCommand.Source
    }
}

if (-not $innoCompiler) {
    throw 'Inno Setup 6.6 or later is required. Install it from https://jrsoftware.org/isinfo.php.'
}

$publishArguments = @(
    'publish',
    $projectPath,
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    "-p:Version=$Version",
    '-o', $publishPath
)

& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

& $innoCompiler $installerScript "/DMyAppVersion=$Version" "/DMyPublishDir=$publishPath"
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

$outputPath = Join-Path $PSScriptRoot "Output\CatSit-Setup-$Version.exe"
Write-Host "Installer created: $outputPath"
