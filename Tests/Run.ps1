$ErrorActionPreference='Stop'
$homeDir=Split-Path $PSScriptRoot -Parent
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Path "$homeDir\Verification" -Force | Out-Null
& $compiler /nologo /target:exe "/out:$homeDir\Verification\CoreTests.exe" /reference:System.Core.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll "$PSScriptRoot\CoreTests.cs" "$PSScriptRoot\FlexibilityTests.cs" "$homeDir\Source\Installation.cs" "$homeDir\Source\Customization.cs" "$homeDir\Source\UpdateSchedule.cs" "$homeDir\Source\ProfileStorage.cs" "$homeDir\Source\PreferenceSharing.cs" "$PSScriptRoot\UpdateTests.cs" "$homeDir\Source\UpdateCore.cs" "$homeDir\Source\UpdateService.cs"
if($LASTEXITCODE -ne 0){throw 'Test compilation failed'}
& "$homeDir\Verification\CoreTests.exe"
if($LASTEXITCODE -ne 0){throw 'Tests failed'}
