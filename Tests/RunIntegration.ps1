$ErrorActionPreference='Stop'
$homeDir=Split-Path $PSScriptRoot -Parent
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$fixture=Join-Path $homeDir ('Verification\Engine-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$fixture\fixture-install" -Force | Out-Null
Copy-Item -LiteralPath "$homeDir\Dual Discord.exe" -Destination "$fixture\Dual Discord.exe"
Set-Content -LiteralPath "$fixture\FixtureClient.cs" -Value 'class FixtureClient {static void Main(){System.Threading.Thread.Sleep(60000);}}'
& $compiler /nologo /target:winexe "/out:$fixture\fixture-install\Discord.exe" "$fixture\FixtureClient.cs"
if($LASTEXITCODE -ne 0){throw 'Fixture compilation failed'}
& $compiler /nologo /target:exe "/out:$fixture\EngineIntegrationTests.exe" /reference:System.Core.dll /reference:Microsoft.CSharp.dll "/reference:$fixture\Dual Discord.exe" "$PSScriptRoot\EngineIntegrationTests.cs"
if($LASTEXITCODE -ne 0){throw 'Integration compilation failed'}
& "$fixture\EngineIntegrationTests.exe"
if($LASTEXITCODE -ne 0){throw 'Engine integration failed'}
