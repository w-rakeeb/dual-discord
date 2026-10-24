param([switch]$Personal,[string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Make-Icon.ps1')
& (Join-Path $PSScriptRoot 'Make-DiscordLogo.ps1')
& (Join-Path $PSScriptRoot 'Make-Icon.ps1') -Name 'Dualcord'
$targetDir = Split-Path $PSScriptRoot -Parent
$buildDir=$targetDir;if($OutputDirectory){$buildDir=[IO.Path]::GetFullPath($OutputDirectory);New-Item -ItemType Directory -Path $buildDir -Force | Out-Null;Copy-Item -LiteralPath (Join-Path $targetDir 'Assets') -Destination $buildDir -Recurse -Force}
$frameworkDir = (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319')
$wpfDir = Join-Path $frameworkDir 'WPF'
$sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | Select-Object -ExpandProperty FullName
$editionArgs=@();if($Personal){$editionArgs+='/define:PERSONAL'}
& (Join-Path $frameworkDir 'csc.exe') /nologo /target:winexe /main:App @editionArgs "/resource:$targetDir\Assets\Dualcord.svg,DualDiscord.Dualcord.svg" "/resource:$targetDir\Assets\Dualcord.png,DualDiscord.Dualcord.png" "/resource:$targetDir\Assets\Dualcord.ico,DualDiscord.Dualcord.ico" "/out:$buildDir\Dual Discord.exe" "/win32icon:$targetDir\Assets\Dual Discord.ico" "/win32manifest:$targetDir\Assets\app.manifest" /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Core.dll /reference:System.Management.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:Microsoft.CSharp.dll /reference:System.Web.Extensions.dll "/reference:$wpfDir\WindowsBase.dll" "/reference:$wpfDir\PresentationCore.dll" "/reference:$wpfDir\PresentationFramework.dll" "/reference:$frameworkDir\System.Xaml.dll" $sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
& (Join-Path $frameworkDir 'csc.exe') /nologo /target:winexe /main:DualDiscord.UpdaterProgram "/out:$buildDir\Dual Discord Updater.exe" /reference:System.Core.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'UpdaterMain.cs') (Join-Path $PSScriptRoot 'UpdateCore.cs') (Join-Path $PSScriptRoot 'UpdateService.cs') (Join-Path $PSScriptRoot 'AssemblyInfo.cs')
if($LASTEXITCODE -ne 0){throw 'Updater build failed.'}
Set-Content -LiteralPath "$buildDir\Dual Discord.exe.config" -Value '<?xml version="1.0"?><configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/></startup></configuration>'
