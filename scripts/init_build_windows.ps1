$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-Not (Test-Path $vsWhere)) { throw "vsWhere not found." }
$msbuildPath = & $vsWhere -latest -prerelease -property installationPath
if(-Not $?) { throw "vsWhere error." }
$msbuildPath = Split-Path(Resolve-Path (Join-Path $msbuildPath 'MSBuild\*\Bin\MSBuild.exe'))
if (-Not (Test-Path $msbuildPath)) { throw "MSBuild not found." }

$env:Path += ";$msbuildPath"

Write-Host "Path updated."
Write-Host "$msbuildPath"
