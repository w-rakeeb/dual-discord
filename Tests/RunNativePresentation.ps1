param([string]$AppDirectory)
$ErrorActionPreference='Stop'
$homeDir=Split-Path $PSScriptRoot -Parent
if(-not $AppDirectory){$AppDirectory=$homeDir}
$frameworkDir=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$fixture=Join-Path $homeDir ('Verification\Presentation-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $AppDirectory 'Dual Discord.exe') -Destination $fixture
& "$frameworkDir\csc.exe" /nologo /target:exe "/out:$fixture\NativePresentationTests.exe" /reference:System.Core.dll "/reference:$frameworkDir\System.Xaml.dll" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/reference:$frameworkDir\WPF\WindowsBase.dll" "/reference:$frameworkDir\WPF\PresentationCore.dll" "/reference:$frameworkDir\WPF\PresentationFramework.dll" "/reference:$fixture\Dual Discord.exe" "$PSScriptRoot\NativePresentationTests.cs"
if($LASTEXITCODE -ne 0){throw 'Native presentation test compilation failed'}
& "$fixture\NativePresentationTests.exe" $AppDirectory
if($LASTEXITCODE -ne 0){throw 'Native presentation tests failed'}
