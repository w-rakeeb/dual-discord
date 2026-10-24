$ErrorActionPreference='Stop'
$homeDir=Split-Path $PSScriptRoot -Parent
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Path "$homeDir\Verification" -Force | Out-Null
foreach($edition in 'Public','Personal'){
 $arguments=@('/nologo','/target:exe',"/out:$homeDir\Verification\BuildProfile-$edition.exe",'/reference:System.Web.Extensions.dll')
 if($edition -eq 'Personal'){$arguments+='/define:PERSONAL'}
 & $compiler @arguments "$PSScriptRoot\BuildProfileTests.cs" "$homeDir\Source\Customization.cs" "$homeDir\Source\BuildProfile.cs"
 if($LASTEXITCODE -ne 0){throw "$edition profile tests did not compile"}
 & "$homeDir\Verification\BuildProfile-$edition.exe"
 if($LASTEXITCODE -ne 0){throw "$edition profile tests failed"}
}
